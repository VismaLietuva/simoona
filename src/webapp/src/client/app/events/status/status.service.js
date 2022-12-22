(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('eventStatus', {
            InProgress: 1,
            Finished: 2,
            RegistrationIsClosed: 3,
            Full: 4,
            Join: 5,
        })
        .service('eventStatusService', eventStatusService);

    eventStatusService.$inject = ['eventStatus', 'attendStatus'];

    function eventStatusService(eventStatus, attendStatus) {
        var service = {
            getEventStatus: getEventStatus,
        };
        return service;

        /////////

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }

        function getEventStatus(event, isParticipantsList) {
            if (!event) {
                return 0;
            }

            if (
                !hasDatePassed(event.startDate) &&
                hasDatePassed(event.endDate)
            ) {
                return eventStatus.InProgress;
            } else if (
                !hasDatePassed(event.startDate) &&
                !hasDatePassed(event.endDate)
            ) {
                return eventStatus.Finished;
            } else if (
                !!event.registrationDeadlineDate &&
                !hasDatePassed(event.registrationDeadlineDate)
            ) {
                return eventStatus.RegistrationIsClosed;
            } else if (
                isEventFull(event) && (event.participatingStatus == attendStatus.NotAttending || event.participatingStatus == attendStatus.Idle || !!isParticipantsList)
            ) {
                return eventStatus.Full;
            } else {
                return eventStatus.Join;
            }
        }

        function isEventFull(event) {
            return event.goingCount === undefined && event.virtuallyGoingCount === undefined
                ? event.maxParticipants <= event.participantsCount && event.maxVirtualParticipants <= event.virtualParticipantsCount
                : event.maxParticipants <= event.goingCount && event.maxVirtualParticipants <= event.virtuallyGoingCount
        }
    }
})();
