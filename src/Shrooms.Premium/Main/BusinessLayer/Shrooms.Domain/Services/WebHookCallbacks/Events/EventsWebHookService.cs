using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models.Wall;
using Shrooms.Domain.Services.Events.Calendar;
using Shrooms.Domain.Services.Wall;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Infrastructure.SystemClock;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.WebHookCallbacks.Events
{
    public class EventsWebHookService : IEventsWebHookService
    {
        private static readonly Dictionary<EventRecurrenceOptions, Func<DateTime, DateTime>> RecurrancePeriods = new Dictionary<EventRecurrenceOptions, Func<DateTime, DateTime>>
        {
            { EventRecurrenceOptions.EveryDay, e => e.AddDays(1) },
            { EventRecurrenceOptions.EveryWeek, e => e.AddDays(7) },
            { EventRecurrenceOptions.EveryTwoWeeks, e => e.AddDays(14) },
            { EventRecurrenceOptions.EveryMonth, e => e.AddMonths(1) }
        };

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<EventOption> _eventOptionsDbSet;
        private readonly IUnitOfWork2 _uow;
        private readonly IEventCalendarService _calendarService;
        private readonly ISystemClock _systemClock;
        private readonly IWallService _wallService;

        public EventsWebHookService(
            IUnitOfWork2 uow,
            ISystemClock systemClock,
            IEventCalendarService calendarService,
            IWallService wallService)
        {
            _uow = uow;
            _eventsDbSet = uow.GetDbSet<Event>();
            _eventOptionsDbSet = uow.GetDbSet<EventOption>();

            _systemClock = systemClock;

            _calendarService = calendarService;
            _wallService = wallService;
        }

        public async Task UpdateRecurringEvents()
        {
            var newEvents = new List<Event>();

            var eventsToUpdate = await _eventsDbSet
                .Include(e => e.EventOptions)
                .Include(u => u.ResponsibleUser)
                .Where(e =>
                    e.EventRecurring != EventRecurrenceOptions.None &&
                    e.EndDate < _systemClock.UtcNow)
                .ToListAsync();

            foreach (var @event in eventsToUpdate)
            {
                var newWallId = await CreateEventWall(@event);
                var newEvent = CreateNewEvent(@event, newWallId);
                _eventsDbSet.Add(newEvent);
                @event.EventRecurring = EventRecurrenceOptions.None;
                CreateNewOptions(@event.EventOptions, newEvent);
                newEvents.Add(newEvent);
            }

            await _uow.SaveChangesAsync(false);

            newEvents.ForEach(x => _calendarService.CreateEvent(x, x.OrganizationId.Value));
        }

        private static Event CreateNewEvent(Event @event, int wallId)
        {
            return new Event
            {
                ResponsibleUser = @event.ResponsibleUser,
                Description = @event.Description,
                CreatedBy = @event.CreatedBy,
                EventRecurring = @event.EventRecurring,
                EventTypeId = @event.EventTypeId,
                ImageName = @event.ImageName,
                MaxChoices = @event.MaxChoices,
                MaxParticipants = @event.MaxParticipants,
                OfficeId = @event.OfficeId,
                OrganizationId = @event.OrganizationId,
                Name = @event.Name,
                Modified = @event.Modified,
                ModifiedBy = @event.ModifiedBy,
                Place = @event.Place,
                Created = @event.Created,
                LocalStartDate = RecurrancePeriods[@event.EventRecurring](@event.LocalStartDate),
                LocalEndDate = RecurrancePeriods[@event.EventRecurring](@event.LocalEndDate),
                LocalRegistrationDeadline = RecurrancePeriods[@event.EventRecurring](@event.LocalRegistrationDeadline),
                WallId = wallId
            };
        }

        private async Task<int> CreateEventWall(Event @event)
        {
            var wallDto = new CreateWallDto
            {
                Access = EntityModels.Models.Multiwall.WallAccess.Private,
                Name = @event.Name,
                ModeratorsIds = new List<string> { @event.ResponsibleUserId },
                MembersIds = new List<string> { @event.ResponsibleUserId },
                Type = EntityModels.Models.Multiwall.WallType.Events,
                Logo = @event.ImageName,
                OrganizationId = @event.OrganizationId.Value,
                UserId = @event.ResponsibleUserId,
                Description = @event.Description
            };

            var wallId = await _wallService.CreateNewWall(wallDto);
            return wallId;
        }

        private void CreateNewOptions(IEnumerable<EventOption> expiredEventOptions, Event newEvent)
        {
            var timestamp = _systemClock.UtcNow;
            foreach (var option in expiredEventOptions)
            {
                _eventOptionsDbSet.Add(new EventOption
                {
                    Created = timestamp,
                    Modified = timestamp,
                    CreatedBy = option.CreatedBy,
                    ModifiedBy = option.ModifiedBy,
                    Option = option.Option,
                    Event = newEvent
                });
            }
        }
    }
}
