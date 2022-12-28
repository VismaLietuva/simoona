(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .factory('eventService', eventService);

    eventService.$inject = [ 'attendStatus' ];

    function eventService(attendStatus) {
        var service = {
            countParticipants: countParticipants,
            hasSpaceForVirtualParticipant: hasSpaceForVirtualParticipant,
            hasSpaceForParticipant: hasSpaceForParticipant,
            getTotalGoingParticipantCount: getTotalGoingParticipantCount,
            getTotalMaxParticipantCount: getTotalMaxParticipantCount,
            countVirtuallyAttendingParticipants: countVirtuallyAttendingParticipants,
            countAttendingParticipants: countAttendingParticipants
        };
        return service;

        function countAttendingParticipants(event) {
            return countParticipants(event, attendStatus.Attending);
        }

        function countVirtuallyAttendingParticipants(event) {
            return countParticipants(event, attendStatus.AttendingVirtually);
        }

        function countParticipants(event, status) {
            return event.participants.reduce((sum, participant) => participant.attendStatus == status ? sum + 1 : sum, 0);
        }

        function hasSpaceForVirtualParticipant(event) {
            return event.virtuallyGoingCount < event.maxVirtualParticipants;
        }

        function hasSpaceForParticipant(event) {
            return event.goingCount < event.maxParticipants;
        }

        function getTotalGoingParticipantCount(event) {
            return event.goingCount + event.virtuallyGoingCount;
        }

        function getTotalMaxParticipantCount(event) {
            return event.maxParticipants + event.maxVirtualParticipants;
        }
    }
})();
