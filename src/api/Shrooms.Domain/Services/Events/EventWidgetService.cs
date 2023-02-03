using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.Events;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Events;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Events
{
    public class EventWidgetService : IEventWidgetService
    {
        private readonly DbSet<Event> _eventsDbSet;
        private readonly ISystemClock _systemClock;

        public EventWidgetService(IUnitOfWork2 uow, ISystemClock systemClock)
        {
            _eventsDbSet = uow.GetDbSet<Event>();
            _systemClock = systemClock;
        }

        public async Task<IEnumerable<UpcomingEventWidgetDto>> GetUpcomingEventsAsync(int organizationId, int eventCount = 5)
        {
            CheckIfEventCountIsValid(eventCount);
            return await _eventsDbSet.Include(e => e.EventType)
                .Where(e => e.OrganizationId == organizationId &&
                            e.StartDate > _systemClock.UtcNow &&
                            e.EventType.CanBeDisplayedInUpcomingEventsWidget &&
                            e.IsShownInUpcomingEventsWidget)
                .OrderBy(e => e.RegistrationDeadline)
                .ThenBy(e => e.StartDate)
                .Take(eventCount)
                .Select(e => new UpcomingEventWidgetDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartDate = e.StartDate,
                    RegistrationDeadlineDate = e.RegistrationDeadline,
                    TypeName = e.EventType.Name,
                    PictureId = e.ImageName
                })
                .ToListAsync();
        }

        private static void CheckIfEventCountIsValid(int eventCount)
        {
            if (eventCount < 1)
            {
                throw new ArgumentException($"Argument '{nameof(eventCount)}' has to be greater than 0");
            }
        }
    }
}
