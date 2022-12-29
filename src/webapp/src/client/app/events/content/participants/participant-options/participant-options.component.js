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
            return getParticipantsByStatuses([attendStatus.Attending, attendStatus.AttendingVirtually]);
        }

        function hasCurrentUserSelectedOption(participants) {
            var currentUserId = authService.identity.userId;

            return !!lodash.find(participants, function(obj) {
                return obj.userId === currentUserId;
            });
        }

        function getParticipantsByStatuses(statuses) {
            return vm.participants.filter(participant => statuses.includes(participant.attendStatus));
        }
    }
})();
