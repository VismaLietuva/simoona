﻿using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.Constants.BusinessLayer.Events;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Premium.Constants;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Events;
using Shrooms.Premium.Main.BusinessLayer.DomainExceptions.Event;

namespace Shrooms.Premium.Main.BusinessLayer.DomainServiceValidators.Events
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

        public void CheckIfSingleChoiceSelectedWithRule(IEnumerable<EventOption> options, OptionRules rule)
        {
            if (options.Any(op => op.Rule == rule) && options.Count() > 1)
            {
                throw new EventException(PremiumErrorCodes.EventChoiceCanBeSingleOnly);
            }
        }

        public void CheckIfUserExistsInOtherSingleJoinEvent(IEnumerable<Event> userParticipationEvent)
        {
            if (userParticipationEvent.Any())
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

        public void CheckIfUserAlreadyJoinedSameEvent(bool isParticipating)
        {
            if (isParticipating)
            {
                throw new EventException(PremiumErrorCodes.EventUserAlreadyParticipatesCode);
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
            if (optionsCount != 0)
            {
                if (maxChoices > optionsCount || optionsCount < BusinessLayerConstants.EventOptionsMinimumCount)
                {
                    throw new EventException(PremiumErrorCodes.EventInsufficientOptionsCode);
                }
            }
        }

        public void CheckIfCreatingEventHasNoChoices(int maxChoices, int eventOptionsCount)
        {
            if (eventOptionsCount != 0)
            {
                if (maxChoices < 1)
                {
                    throw new EventException(PremiumErrorCodes.EventNeedToHaveMaxChoiceCode);
                }
            }
        }

        public void CheckIfTypeDoesNotExist(bool eventTypeExists)
        {
            if (!eventTypeExists)
            {
                throw new EventException(PremiumErrorCodes.EventTypeDoesNotExistCode);
            }
        }

        public void CheckIfAttendStatusIsValid(int status)
        {
            if (!Enum.IsDefined(typeof(BusinessLayerConstants.AttendingStatus), status))
            {
                throw new EventException(PremiumErrorCodes.EventWrongAttendStatus);
            }
        }

        public void CheckIfUserHasPermission(string userId, string responsibleUserId, bool hasPermission)
        {
            if (userId != responsibleUserId)
            {
                if (!hasPermission)
                {
                    throw new EventException(PremiumErrorCodes.EventDontHavePermissionCode);
                }
            }
        }

        public void CheckIfUserHasPermissionToPin(bool newPinStatus, bool currentPinStatus, bool hasPermission)
        {
            if (newPinStatus != currentPinStatus)
            {
                if (!hasPermission)
                {
                    throw new EventException(PremiumErrorCodes.EventDontHavePermissionCode);
                }
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
            else
            {
                if (participant.Event == null)
                {
                    throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Event does not exist");
                }
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

        public void CheckIfOptionsAreDifferent(IEnumerable<NewEventOptionDTO> options)
        {
            if (options != null)
            {
                var duplicateKeys = options.GroupBy(x => x.Option)
                        .Where(group => group.Count() > 1)
                        .Select(group => group.Key);

                if (duplicateKeys.Any())
                {
                    throw new EventException(PremiumErrorCodes.EventOptionsCantDuplicate);
                }
            }
        }

        public void CheckIfEventHasParticipants(IEnumerable<EventParticipantDTO> eventParticipants)
        {
            if (!eventParticipants.Any())
            {
                throw new EventException(PremiumErrorCodes.EventParticipantsNotFound);
            }
        }

        public void CheckIfUserParticipatesInEvent(string userId, IEnumerable<string> participantIds)
        {
            if (participantIds.All(p => p != userId))
            {
                throw new EventException(PremiumErrorCodes.EventUserNotParticipating);
            }
        }
    }
}
