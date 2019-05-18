using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Infrastructure.SystemClock;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.Shrooms.DomainExceptions.Exceptions.Event;
using Shrooms.Premium.Other.Shrooms.Constants.ErrorCodes;

namespace Shrooms.Premium.Main.BusinessLayer.Shrooms.DomainServiceValidators.Validators.Events
{
    public class EventValidationService : IEventValidationService
    {
        private readonly ISystemClock _systemClock;

        public EventValidationService(ISystemClock systemClock)
        {
            _systemClock = systemClock;
        }

        public void CheckIfParticipantExists(object participant)
        {
            if (participant == null)
            {
                throw new EventException(ErrorCodes.EventParticipantNotFound);
            }
        }

        public void CheckIfProvidedOptionsAreValid(IEnumerable<int> providedOptions, IEnumerable<EventOption> foundOptions)
        {
            if (providedOptions.Count() != foundOptions.Count())
            {
                throw new EventException(ErrorCodes.EventNoSuchOptionsCode);
            }
        }

        public void CheckIfUserExistsInOtherSingleJoinEvent(Event userParticipationEvent)
        {
            if (userParticipationEvent != null)
            {
                throw new EventException(ErrorCodes.EventCannotJoinMultipleSingleJoinEventsCode);
            }
        }

        public void CheckIfJoiningEventStartDateHasPassed(DateTime startDate)
        {
            if (startDate < _systemClock.UtcNow)
            {
                throw new EventException(ErrorCodes.EventJoinStartDateHasPassedCode);
            }
        }

        public void CheckIfUserAlreadyJoinedSameEvent(bool isParticipating)
        {
            if (isParticipating)
            {
                throw new EventException(ErrorCodes.EventUserAlreadyParticipatesCode);
            }
        }

        public void CheckIfEventIsFull(int maxParticipants, int participantsCount)
        {
            if (maxParticipants <= participantsCount)
            {
                throw new EventException(ErrorCodes.EventIsFullCode);
            }
        }

        public void CheckIfJoiningTooManyChoicesProvided(int maxChoices, int choicesProvidedCount)
        {
            if (maxChoices < choicesProvidedCount)
            {
                throw new EventException(ErrorCodes.EventTooManyChoicesProvidedCode);
            }
        }

        public void CheckIfEventEndDateIsExpired(DateTime endDate)
        {
            if (endDate < _systemClock.UtcNow)
            {
                throw new EventException(ErrorCodes.EventHasAlreadyExpiredCode);
            }
        }

        public void CheckIfJoiningNotEnoughChoicesProvided(int maxChoices, int choicesProvidedCount)
        {
            if (maxChoices > 0 && choicesProvidedCount == 0)
            {
                throw new EventException(ErrorCodes.EventNotEnoughChoicesProvidedCode);
            }
        }

        public void CheckIfEventStartDateIsExpired(DateTime startDate)
        {
            if (_systemClock.UtcNow > startDate)
            {
                throw new EventException(ErrorCodes.EventCreateStartDateIncorrectCode);
            }
        }

        public void CheckIfRegistrationDeadlineIsExpired(DateTime registrationDeadline)
        {
            if (_systemClock.UtcNow > registrationDeadline)
            {
                throw new EventException(ErrorCodes.EventRegistrationDeadlineIsExpired);
            }
        }

        public void CheckIfRegistrationDeadlineExceedsStartDate(DateTime registrationDeadline, DateTime startDate)
        {
            if (startDate < registrationDeadline)
            {
                throw new EventException(ErrorCodes.EventRegistrationDeadlineGreaterThanStartDateCode);
            }
        }

        public void CheckIfEndDateIsGreaterThanStartDate(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new EventException(ErrorCodes.EventStartDateGreaterThanEndDateCode);
            }
        }

        public void CheckIfResponsibleUserNotExists(bool userExists)
        {
            if (!userExists)
            {
                throw new EventException(ErrorCodes.EventResponsiblePersonDoesNotExistCode);
            }
        }

        public void CheckIfCreatingEventHasInsufficientOptions(int maxChoices, int optionsCount)
        {
            if (optionsCount != 0)
            {
                if (maxChoices > optionsCount || optionsCount < BusinessLayerConstants.EventOptionsMinimumCount)
                {
                    throw new EventException(ErrorCodes.EventInsufficientOptionsCode);
                }
            }
        }

        public void CheckIfCreatingEventHasNoChoices(int maxChoices, int eventOptionsCount)
        {
            if (eventOptionsCount != 0)
            {
                if (maxChoices < 1)
                {
                    throw new EventException(ErrorCodes.EventNeedToHaveMaxChoiceCode);
                }
            }
        }

        public void CheckIfTypeDoesNotExist(bool eventTypeExists)
        {
            if (!eventTypeExists)
            {
                throw new EventException(ErrorCodes.EventTypeDoesNotExistCode);
            }
        }

        public void CheckIfUserHasPermission(string userId, string responsibleUserId, bool hasPermission)
        {
            if (userId != responsibleUserId)
            {
                if (!hasPermission)
                {
                    throw new EventException(ErrorCodes.EventDontHavePermissionCode);
                }
            }
        }

        public void CheckIfEventExists(object @event)
        {
            if (@event == null)
            {
                throw new EventException(ErrorCodes.EventDoesNotExistCode);
            }
        }

        public void CheckIfEventExists(EventParticipant participant)
        {
            if (participant == null)
            {
                throw new ValidationException(global::Shrooms.Constants.ErrorCodes.ErrorCodes.ContentDoesNotExist, "Event does not exist");
            }
            else
            {
                if (participant.Event == null)
                {
                    throw new ValidationException(global::Shrooms.Constants.ErrorCodes.ErrorCodes.ContentDoesNotExist, "Event does not exist");
                }
            }
        }

        public void CheckIfEventHasEnoughPlaces(int maxParticipants, int participantsCount)
        {
            if (maxParticipants < participantsCount)
            {
                throw new EventException(ErrorCodes.EventIsFullCode);
            }
        }

        public void CheckIfUserExists(bool userExists)
        {
            if (!userExists)
            {
                throw new EventException(ErrorCodes.EventJoinUserDoesNotExists);
            }
        }

        public void CheckIfOptionsAreDifferent(IEnumerable<string> options)
        {
            if (options == null)
            {
                return;
            }

            var duplicateKeys = options.GroupBy(x => x)
                .Where(group => @group.Count() > 1)
                .Select(group => @group.Key);

            if (duplicateKeys.Any())
            {
                throw new EventException(ErrorCodes.EventOptionsCantDuplicate);
            }
        }

        public void CheckIfEventHasParticipants(IEnumerable<EventParticipantDTO> eventParticipants)
        {
            if (!eventParticipants.Any())
            {
                throw new EventException(ErrorCodes.EventParticipantsNotFound);
            }
        }
    }
}
