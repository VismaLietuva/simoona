using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.Models.Events;

namespace Shrooms.Premium.Domain.DomainServiceValidators.Events
{
    public interface IEventValidationService
    {
        void CheckIfUserExists(bool userExists);
        void CheckIfEventExists(object eventToJoin);
        void CheckIfEventExists(EventParticipant participant);
        void CheckIfParticipantExists(object participant);
        void CheckIfTypeDoesNotExist(bool eventTypeExists);
        void CheckIfResponsibleUserNotExists(bool userExists);
        void CheckIfEventEndDateIsExpired(DateTime endDate);
        void CheckIfEventStartDateIsExpired(DateTime startDate);
        void CheckIfOptionsAreDifferent(IEnumerable<NewEventOptionDto> options);
        void CheckIfSingleChoiceSelectedWithRule(ICollection<EventOption> options, OptionRules rule);
        void CheckIfJoiningEventStartDateHasPassed(DateTime startDate);
        void CheckIfEventIsFull(int maxParticipants, int participantsCount);
        void CheckIfAttendStatusIsValid(AttendingStatus status);
        void CheckIfJoinAttendStatusIsValid(AttendingStatus status, EventJoinValidationDto @event);
        void CheckIfAttendOptionIsAllowed(AttendingStatus status, EventJoinValidationDto @event);
        void CheckIfAttendOptionsAllowedToUpdate(EditEventDto eventDto, Event eventToUpdate);
        void CheckIfRegistrationDeadlineIsExpired(DateTime registrationDeadline);
        void CheckIfUserExistsInOtherSingleJoinEvent(bool anyEvents);
        void CheckIfEventHasEnoughPlaces(int maxParticipants, int participantCount);
        void CheckIfCanJoinEvent(EventJoinDto joinDto, EventJoinValidationDto joinValidationDto);
        void CheckIfJoiningTooManyChoicesProvided(int maxChoices, int choicesTaken);
        void CheckIfCreatingEventHasNoChoices(int maxChoices, int eventOptionsCount);
        void CheckIfEndDateIsGreaterThanStartDate(DateTime startDate, DateTime endDate);
        void CheckIfJoiningNotEnoughChoicesProvided(int maxChoices, int choicesProvided);
        void CheckIfCreatingEventHasInsufficientOptions(int maxChoices, int optionsCount);
        void CheckIfUserHasPermission(string userId, string responsibleUserId, bool isAdmin);
        void CheckIfUserHasPermissionToPin(bool newPinStatus, bool currentPinStatus, bool isAdmin);
        void CheckIfRegistrationDeadlineExceedsStartDate(DateTime registrationDeadline, DateTime startDate);
        void CheckIfProvidedOptionsAreValid(IEnumerable<int> providedOptionsCount, IEnumerable<EventOption> foundOptionsCount);
        void CheckIfEventHasParticipants(IEnumerable<EventParticipantDto> eventParticipants);
        void CheckIfUserParticipatesInEvent(string userId, IEnumerable<EventParticipantAttendDto> participants);
        void CheckIfDateRangeExceededLimitOrNull(DateTime? start, DateTime? end);
        void CheckIfAllParticipantsExist(ICollection<ApplicationUser> users, ICollection<string> participantIds);
        void CheckIfEventReminderCanBeRemoved(EventReminder reminder);
        void CheckIfEventReminderCanBeUpdated(EventReminder reminder);
    }
}
