using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Domain.DomainExceptions.Event;

namespace Shrooms.Premium.Domain.Services.Events.Participation
{
    public class EventSeatLock : IEventSeatLock
    {
        private const int LockTimeoutMilliseconds = 15000;

        private readonly DbSet<Event> _events;
        private readonly ILogger<EventSeatLock> _logger;

        // Scoped service: one instance per request, so plain fields carry the held-lock state.
        private Guid? _heldEventId;
        private List<Action> _afterCommit;

        public EventSeatLock(IUnitOfWork2 uow, ILogger<EventSeatLock> logger)
        {
            _events = uow.GetDbSet<Event>();
            _logger = logger;
        }

        public async Task RunAsync(Guid eventId, Func<Task> work)
        {
            if (_heldEventId.HasValue)
            {
                if (_heldEventId.Value != eventId)
                {
                    throw new InvalidOperationException($"Event {eventId} seat lock requested while holding the lock of event {_heldEventId}");
                }

                await work();
                return;
            }

            var pending = new List<Action>();
            _heldEventId = eventId;
            _afterCommit = pending;

            try
            {
                await RunLockedAsync(eventId, work);
            }
            finally
            {
                _heldEventId = null;
                _afterCommit = null;
            }

            foreach (var action in pending)
            {
                action();
            }
        }

        public void AfterCommit(Action action)
        {
            if (_afterCommit == null)
            {
                action();
                return;
            }

            _afterCommit.Add(action);
        }

        private async Task RunLockedAsync(Guid eventId, Func<Task> work)
        {
            var database = _events.GetService<ICurrentDbContext>().Context.Database;
            if (!database.IsRelational())
            {
                await work();
                return;
            }

            await using var transaction = await database.BeginTransactionAsync();

            var result = new SqlParameter("@result", SqlDbType.Int) { Direction = ParameterDirection.Output };
            await database.ExecuteSqlRawAsync(
                "EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = @timeout",
                result,
                new SqlParameter("@resource", $"EventSeats:{eventId}"),
                new SqlParameter("@timeout", SqlDbType.Int) { Value = LockTimeoutMilliseconds });

            var lockResult = (int)result.Value;
            if (lockResult < 0)
            {
                // -1 timeout, -2 cancelled, -3 deadlock victim, -999 parameter or call error
                _logger.LogWarning("Could not take the seat lock of event {EventId}: sp_getapplock returned {Result}", eventId, lockResult);
                throw new EventException(PremiumErrorCodes.EventSeatsBusy);
            }

            await work();
            await transaction.CommitAsync();
        }
    }
}
