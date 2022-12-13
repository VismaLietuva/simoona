(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .factory('eventService', eventService);

    function eventService() {
        var service = {
            countParticipants: countParticipants
        };
        return service;

        function countParticipants(event, attendStatus) {
            return event.participants.reduce((sum, participant) => participant.attendStatus == attendStatus ? sum + 1 : 0, 0);
        }
    }
})();
