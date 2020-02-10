﻿using Shrooms.Constants.BusinessLayer;
using Shrooms.DomainExceptions.Exceptions.Event;
using Shrooms.EntityModels.Models.Events;
using Shrooms.Infrastructure.SystemClock;
using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.DataTransferObjects.Models.Events;
using static Shrooms.Constants.ErrorCodes.ErrorCodes;
using static Shrooms.Premium.Other.Shrooms.Constants.ErrorCodes.ErrorCodes;
using Shrooms.DomainExceptions.Exceptions;
using Shrooms.Constants.BusinessLayer.Events;

namespace Shrooms.DomainServiceValidators.Validators.Events
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
                throw new EventException(EventParticipantNotFound);
            }
        }

        public void CheckIfProvidedOptionsAreValid(IEnumerable<int> providedOptions, IEnumerable<EventOption> foundOptions)
        {
            if (providedOptions.Count() != foundOptions.Count())
            {
                throw new EventException(EventNoSuchOptionsCode);
            }
        }

        public void CheckIfSingleChoiceSelectedWithRule(IEnumerable<EventOption> options, OptionRules rule)
        {
            if (options.Any(op => op.Rule == rule) && options.Count() > 1)
            {
                throw new EventException(EventChoiceCanBeSingleOnly);
            }
        }

        public void CheckIfUserExistsInOtherSingleJoinEvent(bool anyEvents)
        {
            if (anyEvents)
            {
                throw new EventException(EventCannotJoinMultipleSingleJoinEventsCode);
            }
        }

        public void CheckIfJoiningEventStartDateHasPassed(DateTime startDate)
        {
            if (startDate < _systemClock.UtcNow)
            {
                throw new EventException(EventJoinStartDateHasPassedCode);
            }
        }

        public void CheckIfUserAlreadyJoinedSameEvent(bool isParticipating)
        {
            if (isParticipating)
            {
                throw new EventException(EventUserAlreadyParticipatesCode);
            }
        }

        public void CheckIfEventIsFull(int maxParticipants, int participantsCount)
        {
            if (maxParticipants <= participantsCount)
            {
                throw new EventException(EventIsFullCode);
            }
        }

        public void CheckIfJoiningTooManyChoicesProvided(int maxChoices, int choicesProvidedCount)
        {
            if (maxChoices < choicesProvidedCount)
            {
                throw new EventException(EventTooManyChoicesProvidedCode);
            }
        }

        public void CheckIfEventEndDateIsExpired(DateTime endDate)
        {
            if (endDate < _systemClock.UtcNow)
            {
                throw new EventException(EventHasAlreadyExpiredCode);
            }
        }

        public void CheckIfJoiningNotEnoughChoicesProvided(int maxChoices, int choicesProvidedCount)
        {
            if (maxChoices > 0 && choicesProvidedCount == 0)
            {
                throw new EventException(EventNotEnoughChoicesProvidedCode);
            }
        }

        public void CheckIfEventStartDateIsExpired(DateTime startDate)
        {
            if (_systemClock.UtcNow > startDate)
            {
                throw new EventException(EventCreateStartDateIncorrectCode);
            }
        }

        public void CheckIfRegistrationDeadlineIsExpired(DateTime registrationDeadline)
        {
            if (_systemClock.UtcNow > registrationDeadline)
            {
                throw new EventException(EventRegistrationDeadlineIsExpired);
            }
        }

        public void CheckIfRegistrationDeadlineExceedsStartDate(DateTime registrationDeadline, DateTime startDate)
        {
            if (startDate < registrationDeadline)
            {
                throw new EventException(EventRegistrationDeadlineGreaterThanStartDateCode);
            }
        }

        public void CheckIfEndDateIsGreaterThanStartDate(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new EventException(EventStartDateGreaterThanEndDateCode);
            }
        }

        public void CheckIfResponsibleUserNotExists(bool userExists)
        {
            if (!userExists)
            {
                throw new EventException(EventResponsiblePersonDoesNotExistCode);
            }
        }

        public void CheckIfCreatingEventHasInsufficientOptions(int maxChoices, int optionsCount)
        {
            if (optionsCount != 0 && (maxChoices > optionsCount || optionsCount < ConstBusinessLayer.EventOptionsMinimumCount))
            {
                throw new EventException(EventInsufficientOptionsCode);
            }
        }

        public void CheckIfCreatingEventHasNoChoices(int maxChoices, int eventOptionsCount)
        {
            if (eventOptionsCount != 0 && maxChoices < 1)
            {
                throw new EventException(EventNeedTohaveMaxChoiceCode);
            }
        }

        public void CheckIfTypeDoesNotExist(bool eventTypeExists)
        {
            if (!eventTypeExists)
            {
                throw new EventException(EventTypeDoesNotExistCode);
            }
        }

        public void CheckIfAttendStatusIsValid(int status)
        {
            if (!Enum.IsDefined(typeof(ConstBusinessLayer.AttendingStatus), status))
            {
                throw new EventException(EventWrongAttendStatus);

            }
        }

        public void CheckIfUserHasPermission(string userId, string responsibleUserId, bool hasPermission)
        {
            if (userId != responsibleUserId && !hasPermission)
            {
                throw new EventException(EventDontHavePermissionCode);
            }
        }

        public void CheckIfUserHasPermissionToPin(bool newPinStatus, bool currentPinStatus, bool hasPermission)
        {
            if (newPinStatus != currentPinStatus && !hasPermission)
            {
                throw new EventException(EventDontHavePermissionCode);
            }
        }

        public void CheckIfEventExists(object @event)
        {
            if (@event == null)
            {
                throw new EventException(EventDoesNotExistCode);
            }
        }

        public void CheckIfEventExists(EventParticipant participant)
        {
            if (participant == null)
            {
                throw new ValidationException(ContentDoesNotExist, "Event does not exist");
            }

            if (participant.Event == null)
            {
                throw new ValidationException(ContentDoesNotExist, "Event does not exist");
            }
        }

        public void CheckIfEventHasEnoughPlaces(int maxParticipants, int participantsCount)
        {
            if (maxParticipants < participantsCount)
            {
                throw new EventException(EventIsFullCode);
            }
        }

        public void CheckIfUserExists(bool userExists)
        {
            if (!userExists)
            {
                throw new EventException(EventJoinUserDoesNotExists);
            }
        }

        public void CheckIfOptionsAreDifferent(IEnumerable<NewEventOptionDTO> options)
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
                throw new EventException(EventOptionsCantDuplicate);
            }
        }

        public void CheckIfEventHasParticipants(IEnumerable<EventParticipantDTO> eventParticipants)
        {
            if (!eventParticipants.Any())
            {
                throw new EventException(EventParticipantsNotFound);
            }
        }

        public void CheckIfUserParticipatesInEvent(string userId, IEnumerable<string> participantIds)
        {
            if (participantIds.All(p => p != userId))
            {
                throw new EventException(EventUserNotParticipating);
            }
        }
    }
}
