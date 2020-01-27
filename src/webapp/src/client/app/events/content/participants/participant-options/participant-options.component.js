(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventParticipantOptions', {
            bindings: {
                options: '=',
                participants: '='
            },
            templateUrl: 'app/events/content/participants/participant-options/participant-options.html',
            controller: eventParticipantOptionsController,
            controllerAs: 'vm'
        });

    eventParticipantOptionsController.$inject = [
        'authService',
        'lodash',
        'attendStatus'
    ];

    function eventParticipantOptionsController(authService, lodash, attendStatus) {
        /* jshint validthis: true */
        var vm = this;

        vm.accordionOptionArray = [];
        vm.hasCurrentUserSelectedOption = hasCurrentUserSelectedOption;
        vm.attendingParticipants = getAttendingParticipants;

        ////////

        function getAttendingParticipants() {
            return getParticipantsByStatus(attendStatus.Attending);
        }

        function hasCurrentUserSelectedOption(participants) {
            var currentUserId = authService.identity.userId;

            return !!lodash.find(participants, function(obj) {
                return obj.userId === currentUserId;
            });
        }

        function getParticipantsByStatus(status) {
            var participantsByStatus = [];
            vm.participants.forEach(function(participant){
                if (participant.attendStatus == status) {
                    participantsByStatus.push(participant);
                }
            })
            return participantsByStatus;
        }
    }
})();
