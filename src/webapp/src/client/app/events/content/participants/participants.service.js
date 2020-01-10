(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .service('eventParticipantsService', eventParticipantsService);

    eventParticipantsService.$inject = [
        'lodash'
    ];

    function eventParticipantsService(lodash) {
        var service = {
            removeParticipant: removeParticipant,
            removeParticipantFromOptions: removeParticipantFromOptions
        };
        return service;

        /////////

        function removeParticipant(participantList, userId) {
            lodash.remove(participantList, function(participant) {
                return participant.userId === userId && participant.attendStatus == 1;
            });
        }

        function removeParticipantFromOptions(options, userId) {
            for (var i = 0; options.length > i; i++) {
                removeParticipant(options[i].participants, userId);
            }
        }
    }
})();
