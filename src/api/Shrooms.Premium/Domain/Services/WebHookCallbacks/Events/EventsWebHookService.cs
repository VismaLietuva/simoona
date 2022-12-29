using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Wall;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Domain.Services.Wall;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Events
{
    public class EventsWebHookService : IEventsWebHookService
    {
        private static readonly Dictionary<EventRecurrenceOptions, Func<DateTime, DateTime>> _recurrencePeriods = new Dictionary<EventRecurrenceOptions, Func<DateTime, DateTime>>
        {
            { EventRecurrenceOptions.EveryDay, e => e.AddDays(1) },
            { EventRecurrenceOptions.EveryWeek, e => e.AddDays(7) },
            { EventRecurrenceOptions.EveryTwoWeeks, e => e.AddDays(14) },
            { EventRecurrenceOptions.EveryMonth, e => e.AddMonths(1) }
        };

        private readonly IDbSet<Event> _eventsDbSet;
        private readonly IDbSet<EventOption> _eventOptionsDbSet;
        private readonly IUnitOfWork2 _uow;
        private readonly ISystemClock _systemClock;
        private readonly IWallService _wallService;
        private readonly IApplicationSettings _appSettings;

        public EventsWebHookService(IUnitOfWork2 uow, ISystemClock systemClock, IWallService wallService, IApplicationSettings appSettings)
        {
            _uow = uow;
            _eventsDbSet = uow.GetDbSet<Event>();
            _eventOptionsDbSet = uow.GetDbSet<EventOption>();

            _systemClock = systemClock;
            _wallService = wallService;
            _appSettings = appSettings;
        }

        public async Task UpdateRecurringEventsAsync()
        {
            var eventsToUpdate = await _eventsDbSet
                    .Include(e => e.EventOptions)
                    .Include(u => u.ResponsibleUser)
                    .Where(e => e.EventRecurring != EventRecurrenceOptions.None && e.EndDate < _systemClock.UtcNow && e.ResponsibleUser != null)
                    .ToListAsync();

            foreach (var @event in eventsToUpdate)
            {
                var newWallId = await CreateEventWallAsync(@event);
                var newEvent = CreateNewEvent(@event, newWallId);
                _eventsDbSet.Add(newEvent);
                @event.EventRecurring = EventRecurrenceOptions.None;
                CreateNewOptions(@event.EventOptions, newEvent);
            }

            await _uow.SaveChangesAsync(false);
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
                MaxVirtualParticipants = @event.MaxVirtualParticipants,
                Offices = @event.Offices,
                OrganizationId = @event.OrganizationId,
                Name = @event.Name,
                Modified = @event.Modified,
                ModifiedBy = @event.ModifiedBy,
                Place = @event.Place,
                Created = @event.Created,
                LocalStartDate = _recurrencePeriods[@event.EventRecurring](@event.LocalStartDate),
                LocalEndDate = _recurrencePeriods[@event.EventRecurring](@event.LocalEndDate),
                LocalRegistrationDeadline = _recurrencePeriods[@event.EventRecurring](@event.LocalRegistrationDeadline),
                WallId = wallId
            };
        }

        private async Task<int> CreateEventWallAsync([NotNull]Event @event)
        {
            var wallDto = new CreateWallDto
            {
                Access = WallAccess.Private,
                Name = @event.Name,
                ModeratorsIds = new List<string> { @event.ResponsibleUserId },
                MembersIds = new List<string> { @event.ResponsibleUserId },
                Type = WallType.Events,
                Logo = @event.ImageName,
                OrganizationId = @event.OrganizationId ?? _appSettings.DefaultOrganizationId,
                UserId = @event.ResponsibleUserId,
                Description = @event.Description
            };

            var wallId = await _wallService.CreateNewWallAsync(wallDto);
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