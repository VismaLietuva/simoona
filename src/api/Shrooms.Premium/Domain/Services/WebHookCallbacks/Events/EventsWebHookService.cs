using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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

        private readonly DbSet<Event> _eventsDbSet;
        private readonly DbSet<EventOption> _eventOptionsDbSet;
        private readonly IUnitOfWork2 _uow;
        private readonly ISystemClock _systemClock;
        private readonly DbSet<EventQuestion> _questionsDbSet;
        private readonly IWallService _wallService;
        private readonly IApplicationSettings _appSettings;

        public EventsWebHookService(IUnitOfWork2 uow, ISystemClock systemClock, IWallService wallService, IApplicationSettings appSettings)
        {
            _uow = uow;
            _eventsDbSet = uow.GetDbSet<Event>();
            _eventOptionsDbSet = uow.GetDbSet<EventOption>();
            _questionsDbSet = uow.GetDbSet<EventQuestion>();

            _systemClock = systemClock;
            _wallService = wallService;
            _appSettings = appSettings;
        }

        public async Task UpdateRecurringEventsAsync()
        {
            var eventsToUpdate = await _eventsDbSet
                    .Include(e => e.EventOptions)
                    .Include(e => e.EventQuestions).ThenInclude(q => q.Options)
                    .Include(u => u.ResponsibleUser)
                    .Where(e => e.EventRecurring != EventRecurrenceOptions.None && e.EndDate < _systemClock.UtcNow && e.ResponsibleUser != null)
                    .ToListAsync();

            foreach (var @event in eventsToUpdate)
            {
                var newWallId = await CreateEventWallAsync(@event);
                var newEvent = CreateNewEvent(@event, newWallId, _systemClock.UtcNow);
                _eventsDbSet.Add(newEvent);
                @event.EventRecurring = EventRecurrenceOptions.None;
                CreateNewOptions(@event.EventOptions, newEvent);
                CreateNewQuestions(@event.EventQuestions, newEvent);
            }

            await _uow.SaveChangesAsync(false);
        }

        private static Event CreateNewEvent(Event @event, int wallId, DateTime utcNow)
        {
            // IsPinned is deliberately left behind: pinning curates one occurrence, not the series.
            var newEvent = new Event
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
                AllowMaybeGoing = @event.AllowMaybeGoing,
                AllowNotGoing = @event.AllowNotGoing,
                IsShownInUpcomingEventsWidget = @event.IsShownInUpcomingEventsWidget,
                OfficeId = @event.OfficeId,
                Offices = @event.Offices,
                OrganizationId = @event.OrganizationId,
                Name = @event.Name,
                Modified = utcNow,
                ModifiedBy = @event.ModifiedBy,
                Place = @event.Place,
                Created = utcNow,
                LocalStartDate = @event.LocalStartDate,
                LocalEndDate = @event.LocalEndDate,
                LocalRegistrationDeadline = @event.LocalRegistrationDeadline,
                WallId = wallId
            };

            // Shifting local dates keeps the wall-clock time across DST; repeating until the
            // occurrence is in the future catches a missed schedule up in a single run.
            var shiftByOnePeriod = _recurrencePeriods[@event.EventRecurring];

            do
            {
                newEvent.LocalStartDate = shiftByOnePeriod(newEvent.LocalStartDate);
                newEvent.LocalEndDate = shiftByOnePeriod(newEvent.LocalEndDate);
                newEvent.LocalRegistrationDeadline = shiftByOnePeriod(newEvent.LocalRegistrationDeadline);
            }
            while (newEvent.EndDate < utcNow);

            return newEvent;
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

            // Legacy flat options only. A question-owned option cloned loose would surface on the
            // next occurrence as a top-level food choice, and MaxChoices does not account for it.
            foreach (var option in expiredEventOptions.Where(option => option.QuestionId == null))
            {
                _eventOptionsDbSet.Add(new EventOption
                {
                    Created = timestamp,
                    Modified = timestamp,
                    CreatedBy = option.CreatedBy,
                    ModifiedBy = option.ModifiedBy,
                    Option = option.Option,
                    Rule = option.Rule,
                    Order = option.Order,
                    Event = newEvent
                });
            }
        }

        /// <summary>
        /// Clones the question tree onto the next occurrence. Conditions are rewired through the
        /// navigation property, since the cloned options have no identity until SaveChanges.
        /// </summary>
        private void CreateNewQuestions(IEnumerable<EventQuestion> expiredQuestions, Event newEvent)
        {
            var timestamp = _systemClock.UtcNow;
            var clonedOptionByOldId = new Dictionary<int, EventOption>();
            var clones = new List<(EventQuestion Old, EventQuestion New)>();

            foreach (var question in expiredQuestions.OrderBy(question => question.Order).ThenBy(question => question.Id))
            {
                var clone = new EventQuestion
                {
                    Created = timestamp,
                    Modified = timestamp,
                    CreatedBy = question.CreatedBy,
                    ModifiedBy = question.ModifiedBy,
                    Title = question.Title,
                    Order = question.Order,
                    SelectType = question.SelectType,
                    IsRequired = question.IsRequired,
                    Event = newEvent,
                    Options = new List<EventOption>()
                };

                foreach (var option in question.Options ?? new List<EventOption>())
                {
                    var optionClone = new EventOption
                    {
                        Created = timestamp,
                        Modified = timestamp,
                        CreatedBy = option.CreatedBy,
                        ModifiedBy = option.ModifiedBy,
                        Option = option.Option,
                        Rule = option.Rule,
                        Order = option.Order,
                        Event = newEvent,
                        Question = clone
                    };

                    // Reached through clone.Options, so adding the question is enough to insert it.
                    clone.Options.Add(optionClone);
                    clonedOptionByOldId[option.Id] = optionClone;
                }

                _questionsDbSet.Add(clone);
                clones.Add((question, clone));
            }

            foreach (var (old, clone) in clones)
            {
                if (old.ShowIfOptionId != null &&
                    clonedOptionByOldId.TryGetValue(old.ShowIfOptionId.Value, out var trigger))
                {
                    clone.ShowIfOption = trigger;
                }
            }
        }
    }
}