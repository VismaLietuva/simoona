(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .factory('eventService', eventService);

    function eventService() {
        var service = {
            countParticipants: countParticipants,
            hasSpaceForVirtualParticipant: hasSpaceForVirtualParticipant,
            hasSpaceForParticipant: hasSpaceForParticipant,
            getTotalGoingParticipantCount: getTotalGoingParticipantCount,
            getTotalMaxParticipantCount: getTotalMaxParticipantCount
        };
        return service;

        function countParticipants(event, attendStatus) {
            return event.participants.reduce((sum, participant) => participant.attendStatus == attendStatus ? sum + 1 : 0, 0);
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
