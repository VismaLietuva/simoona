(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .service('eventParticipantsService', eventParticipantsService);

    eventParticipantsService.$inject = [
        'attendStatus'
    ];

    function eventParticipantsService(attendStatus) {
        var service = {
            removeParticipant: removeParticipant,
            removeParticipantFromOptions: removeParticipantFromOptions
        };
        return service;

        /////////

        function removeParticipant(participantList, userId) {
            participantList.forEach(function(participant) {
                if (userId == participant.userId && participant.attendStatus == attendStatus.Attending)
                {
                    participant.attendStatus = attendStatus.NotAttending;
                }
            });
        }

        function removeParticipantFromOptions(options, userId) {
            for (var i = 0; options.length > i; i++) {
                removeParticipant(options[i].participants, userId);
            }
        }
    }
})();
