(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('eventStatus', {
            InProgress : 1,
            Finished : 2,
            RegistrationIsClosed : 3,
            Full : 4,
            Join : 5
        })
        .service('eventStatusService', eventStatusService);

    eventStatusService.$inject = [
        'eventStatus',
        'attendStatus'
    ];

    function eventStatusService(eventStatus, attendStatus) {
        var service = {
            getEventStatus: getEventStatus
        };
        return service;

        /////////

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }

        function getEventStatus(event, isParticipantsList) {
            if (event) {
                var participantsCount;
                if (!!event.participantsCount) {
                    participantsCount = event.participantsCount;
                }
                else {
                    participantsCount = 0;
                }

                if (!hasDatePassed(event.startDate) && hasDatePassed(event.endDate)) {
                    return eventStatus.InProgress;
                } else if (!hasDatePassed(event.startDate) && !hasDatePassed(event.endDate)) {
                    return eventStatus.Finished;
                } else if (!!event.registrationDeadlineDate && !hasDatePassed(event.registrationDeadlineDate)) {
                    return eventStatus.RegistrationIsClosed;
                } else if (event.maxParticipants <= participantsCount && (event.participatingStatus == attendStatus.NotAttending || !!isParticipantsList)) {
                    return eventStatus.Full;
                } else {
                    return eventStatus.Join;
                }
            }

            return 0;
        }
    }

})();
