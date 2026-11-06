using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        public EventSeatLock(IUnitOfWork2 uow)
        {
            _events = uow.GetDbSet<Event>();
        }

        public async Task RunAsync(Guid eventId, Func<Task> work)
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

            if ((int)result.Value < 0)
            {
                throw new EventException(PremiumErrorCodes.EventIsFullCode);
            }

            await work();
            await transaction.CommitAsync();
        }
    }
}
