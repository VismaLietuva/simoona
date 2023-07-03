using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.DataTransferObjects.Models.Events.Reminders;
using Shrooms.Premium.Domain.DomainExceptions.Event;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    public class EventValidationService : IEventValidationService
    {
        private readonly ISystemClock _systemClock;

        public EventValidationService(ISystemClock systemClock)
        {
            _systemClock = systemClock;
        }

        public void CheckIfAllParticipantsExist(ICollection<ApplicationUser> users, ICollection<string> participantIds)
        {
            if (users.Count != participantIds.Count)
            {
                throw new EventException(PremiumErrorCodes.EventParticipantNotFound);
            }
        }

        public void CheckIfParticipantExists(object participant)
        {
            if (participant == null)
            {
                throw new EventException(PremiumErrorCodes.EventParticipantNotFound);
            }
        }

        public void CheckIfProvidedOptionsAreValid(IEnumerable<int> providedOptions, IEnumerable<EventOption> foundOptions)
        {
            if (providedOptions.Count() != foundOptions.Count())
            {
                throw new EventException(PremiumErrorCodes.EventNoSuchOptionsCode);
            }
        }

        public void CheckIfSingleChoiceSelectedWithRule(ICollection<EventOption> options, OptionRules rule)
        {
            if (options.Any(op => op.Rule == rule) && options.Count > 1)
            {
                throw new EventException(PremiumErrorCodes.EventChoiceCanBeSingleOnly);
            }
        }

        public void CheckIfUserExistsInOtherSingleJoinEvent(bool anyEvents)
        {
            if (anyEvents)
            {
                throw new EventException(PremiumErrorCodes.EventCannotJoinMultipleSingleJoinEventsCode);
            }
        }

        public void CheckIfJoiningEventStartDateHasPassed(DateTime startDate)
        {
            if (startDate < _systemClock.UtcNow)
            {
                throw new EventException(PremiumErrorCodes.EventJoinStartDateHasPassedCode);
            }
        }

        public void CheckIfEventIsFull(int maxParticipants, int participantsCount)
        {
            if (maxParticipants <= participantsCount)
            {
                throw new EventException(PremiumErrorCodes.EventIsFullCode);
            }
        }

        public void CheckIfJoiningTooManyChoicesProvided(int maxChoices, int choicesProvidedCount)
        {
            if (maxChoices < choicesProvidedCount)
            {
                throw new EventException(PremiumErrorCodes.EventTooManyChoicesProvidedCode);
            }
        }

        public void CheckIfEventEndDateIsExpired(DateTime endDate)
        {
            if (endDate < _systemClock.UtcNow)
            {
                throw new EventException(PremiumErrorCodes.EventHasAlreadyExpiredCode);
            }
        }

        public void CheckIfJoiningNotEnoughChoicesProvided(int maxChoices, int choicesProvidedCount)
        {
            if (maxChoices > 0 && choicesProvidedCount == 0)
            {
                throw new EventException(PremiumErrorCodes.EventNotEnoughChoicesProvidedCode);
            }
        }

        public void CheckIfEventStartDateIsExpired(DateTime startDate)
        {
            if (_systemClock.UtcNow > startDate)
            {
                throw new EventException(PremiumErrorCodes.EventCreateStartDateIncorrectCode);
            }
        }

        public void CheckIfRegistrationDeadlineIsExpired(DateTime registrationDeadline)
        {
            if (_systemClock.UtcNow > registrationDeadline)
            {
                throw new EventException(PremiumErrorCodes.EventRegistrationDeadlineIsExpired);
            }
        }

        public void CheckIfRegistrationDeadlineExceedsStartDate(DateTime registrationDeadline, DateTime startDate)
        {
            if (startDate < registrationDeadline)
            {
                throw new EventException(PremiumErrorCodes.EventRegistrationDeadlineGreaterThanStartDateCode);
            }
        }

        public void CheckIfEndDateIsGreaterThanStartDate(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new EventException(PremiumErrorCodes.EventStartDateGreaterThanEndDateCode);
            }
        }

        public void CheckIfResponsibleUserNotExists(bool userExists)
        {
            if (!userExists)
            {
                throw new EventException(PremiumErrorCodes.EventResponsiblePersonDoesNotExistCode);
            }
        }

        public void CheckIfCreatingEventHasInsufficientOptions(int maxChoices, int optionsCount)
        {
            if (optionsCount != 0 && (maxChoices > optionsCount || optionsCount < EventsConstants.EventOptionsMinimumCount))
            {
                throw new EventException(PremiumErrorCodes.EventInsufficientOptionsCode);
            }
        }

        public void CheckIfCreatingEventHasNoChoices(int maxChoices, int eventOptionsCount)
        {
            if (eventOptionsCount != 0 && maxChoices < 1)
            {
                throw new EventException(PremiumErrorCodes.EventNeedToHaveMaxChoiceCode);
            }
        }

        public void CheckIfTypeDoesNotExist(bool eventTypeExists)
        {
            if (!eventTypeExists)
            {
                throw new EventException(PremiumErrorCodes.EventTypeDoesNotExistCode);
            }
        }

        public void CheckIfAttendStatusIsValid(AttendingStatus status)
        {
            if (!Enum.IsDefined(typeof(AttendingStatus), status))
            {
                throw new EventException(PremiumErrorCodes.EventWrongAttendStatus);
            }
        }

        public void CheckIfAttendOptionIsAllowed(AttendingStatus status, EventJoinValidationDto @event)
        {
            if (status == AttendingStatus.MaybeAttending && !@event.AllowMaybeGoing)
            {
                throw new EventException(PremiumErrorCodes.EventAttendTypeIsNotAllowed);
            }

            if (status == AttendingStatus.NotAttending && !@event.AllowNotGoing)
            {
                throw new EventException(PremiumErrorCodes.EventAttendTypeIsNotAllowed);
            }
        }

        public void CheckIfJoinAttendStatusIsValid(AttendingStatus status, EventJoinValidationDto @event)
        {
            if (status != AttendingStatus.AttendingVirtually && status != AttendingStatus.Attending)
            {
                throw new EventException(PremiumErrorCodes.EventAttendTypeIsNotAllowed);
            }

            if (status == AttendingStatus.AttendingVirtually && @event.MaxVirtualParticipants == 0)
            {
                throw new EventException(PremiumErrorCodes.EventAttendTypeIsNotAllowed);
            }

            if (status == AttendingStatus.Attending && @event.MaxParticipants == 0)
            {
                throw new EventException(PremiumErrorCodes.EventAttendTypeIsNotAllowed);
            }
        }

        public void CheckIfAttendOptionsAllowedToUpdate(EditEventDto eventDto, Event eventToUpdate)
        {
            if ((eventDto.AllowMaybeGoing != eventToUpdate.AllowMaybeGoing || eventDto.AllowNotGoing != eventToUpdate.AllowNotGoing) && eventToUpdate.EventParticipants.Count > 0)
            {
                throw new EventException(PremiumErrorCodes.EventAttendTypeCannotBeChangedIfParticipantsJoined);
            }
        }

        public void CheckIfUserHasPermission(string userId, string responsibleUserId, bool hasPermission)
        {
            if (userId != responsibleUserId && !hasPermission)
            {
                throw new EventException(PremiumErrorCodes.EventDontHavePermissionCode);
            }
        }

        public void CheckIfUserHasPermissionToPin(bool newPinStatus, bool currentPinStatus, bool hasPermission)
        {
            if (newPinStatus != currentPinStatus && !hasPermission)
            {
                throw new EventException(PremiumErrorCodes.EventDontHavePermissionCode);
            }
        }

        public void CheckIfEventExists(object @event)
        {
            if (@event == null)
            {
                throw new EventException(PremiumErrorCodes.EventDoesNotExistCode);
            }
        }

        public void CheckIfEventExists(EventParticipant participant)
        {
            if (participant == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event does not exist");
            }

            if (participant.Event == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event does not exist");
            }
        }

        public void CheckIfEventHasEnoughPlaces(int maxParticipants, int participantsCount)
        {
            if (maxParticipants < participantsCount)
            {
                throw new EventException(PremiumErrorCodes.EventIsFullCode);
            }
        }

        public void CheckIfUserExists(bool userExists)
        {
            if (!userExists)
            {
                throw new EventException(PremiumErrorCodes.EventJoinUserDoesNotExists);
            }
        }

        public void CheckIfOptionsAreDifferent(IEnumerable<NewEventOptionDto> options)
        {
            if (options == null)
            {
                return;
            }

            var duplicateKeys = options.GroupBy(x => x.Option)
                .Where(group => @group.Count() > 1)
                .Select(group => @group.Key);

            if (duplicateKeys.Any())
            {
                throw new EventException(PremiumErrorCodes.EventOptionsCantDuplicate);
            }
        }

        public void CheckIfEventHasParticipants(IEnumerable<EventParticipantDto> eventParticipants)
        {
            if (eventParticipants == null || !eventParticipants.Any())
            {
                throw new EventException(PremiumErrorCodes.EventParticipantsNotFound);
            }
        }

        public void CheckIfUserParticipatesInEvent(string userId, IEnumerable<EventParticipantAttendDto> participants)
        {
            if (participants.All(p => p.Id != userId))
            {
                throw new EventException(PremiumErrorCodes.EventUserNotParticipating);
            }
        }

        public void CheckIfDateRangeExceededLimitOrNull(DateTime? start, DateTime? end)
        {
            var diff = end - start;
            if (diff is null || diff > TimeSpan.FromDays(EventsConstants.EventsMaxDateFilterRangeInDays))
            {
                throw new EventException(PremiumErrorCodes.EventDateFilterRangeInvalid);
            }
        }

        public void CheckIfCanJoinEvent(EventJoinDto joinDto, EventJoinValidationDto joinValidationDto)
        {
            var newParticipantCount = joinDto.ParticipantIds.Count();
            var maxParticipantCount = joinDto.AttendStatus == AttendingStatus.Attending ?
                joinValidationDto.MaxParticipants :
                joinValidationDto.MaxVirtualParticipants;
            var participantCount = joinValidationDto.Participants.Count(participant => (AttendingStatus)participant.AttendStatus == joinDto.AttendStatus);

            if (maxParticipantCount < newParticipantCount + participantCount)
            {
                throw new EventException(PremiumErrorCodes.EventIsFullCode);
            }
        }

        public void CheckIfEventReminderCanBeRemoved(IEventArgsDto eventArgsDto, EventReminder reminder, EventRecurrenceOptions newOption)
        {
            if (!CanReminderBeChangedForEvent(eventArgsDto, reminder) && newOption == EventRecurrenceOptions.None)
            {
                throw new EventException(PremiumErrorCodes.EventReminderCannotBeRemoved);
            }
        }

        public void CheckIfEventReminderCanBeUpdated(IEventArgsDto eventArgsDto, EventReminder reminder)
        {
            if (!CanReminderBeChangedForEvent(eventArgsDto, reminder))
            {
                throw new EventException(PremiumErrorCodes.EventReminderCannotBeUpdated);
            }
        }

        private bool CanReminderBeChangedForEvent(IEventArgsDto eventArgsDto, EventReminder reminder)
        {
            return reminder.RemindedCount <= 0 && !IsEventDateForReminderExpired(eventArgsDto, reminder.Type);
        }

        public void CheckIfEventReminderCanBeAdded(IEventArgsDto eventArgsDto, EventReminderDto reminder)
        {
            if (IsEventDateForReminderExpired(eventArgsDto, reminder.Type))
            {
                throw new EventException(PremiumErrorCodes.EventReminderCannotBeAdded);
            }
        }

        private bool IsEventDateForReminderExpired(IEventArgsDto eventArgsDto, EventReminderType type)
        {
            var date = GetDateFromEvent(eventArgsDto, type);
            return date <= _systemClock.UtcNow;
        }

        private static DateTime GetDateFromEvent(IEventArgsDto eventArgsDto, EventReminderType type)
        {
            return type switch
            {
                EventReminderType.Start => eventArgsDto.StartDate,
                EventReminderType.Deadline => eventArgsDto.RegistrationDeadlineDate,
                _ => throw new NotSupportedException($"Unable to get date for reminder of type {type}")
            };
        }
    }
}
