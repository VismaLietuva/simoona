using Shrooms.EntityModels.Models.Events;
using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.Events;

namespace Shrooms.DomainServiceValidators.Validators.Events
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
        void CheckIfOptionsAreDifferent(IEnumerable<string> options);
        void CheckIfUserAlreadyJoinedSameEvent(bool isParticipating);
        void CheckIfJoiningEventStartDateHasPassed(DateTime startDate);
        void CheckIfEventIsFull(int maxParticipants, int participantsCount);
        void CheckIfRegistrationDeadlineIsExpired(DateTime registrationDeadline);
        void CheckIfUserExistsInOtherSingleJoinEvent(Event userParticipationEvent);
        void CheckIfEventHasEnoughPlaces(int maxParticipants, int participantCount);
        void CheckIfJoiningTooManyChoicesProvided(int maxChoices, int choicesTaken);
        void CheckIfCreatingEventHasNoChoices(int maxChoices, int eventOptionsCount);
        void CheckIfEndDateIsGreaterThanStartDate(DateTime startDate, DateTime endDate);
        void CheckIfJoiningNotEnoughChoicesProvided(int maxChoices, int choicesProvided);
        void CheckIfCreatingEventHasInsufficientOptions(int maxChoices, int optionsCount);
        void CheckIfUserHasPermission(string userId, string responsibleUserId, bool isAdmin);
        void CheckIfUserHasPermissionToPin(bool newPinStatus, bool currentPinStatus, bool isAdmin);
        void CheckIfUserHasPermissionToPin(bool pinStatus, bool isAdmin);
        void CheckIfRegistrationDeadlineExceedsStartDate(DateTime registrationDeadline, DateTime startDate);
        void CheckIfProvidedOptionsAreValid(IEnumerable<int> providedOptionsCount, IEnumerable<EventOption> foundOptionsCount);
        void CheckIfEventHasParticipants(IEnumerable<EventParticipantDTO> eventParticipants);
    }
}
