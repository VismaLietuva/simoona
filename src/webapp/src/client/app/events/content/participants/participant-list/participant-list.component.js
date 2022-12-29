(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventParticipantList', {
            bindings: {
                participants: '=',
                onLeaveEvent: '&',
                isDeleteVisible: '=',
                isMainParticipantList: '='
            },
            templateUrl: 'app/events/content/participants/participant-list/participant-list.html',
            controller: eventParticipantListController,
            controllerAs: 'vm'
        });

    eventParticipantListController.inject = [
        'attendStatus'
    ]

    function eventParticipantListController(attendStatus) {
        /* jshint validthis: true */
        var vm = this;
        vm.isExpanded = true;

        vm.attendStatus = attendStatus;
        vm.expandCollapseText = 'events.collapse';
        vm.toggleExpandCollapse = toggleExpandCollapse;
        vm.attendingParticipants = getAttendingParticipants;
        vm.maybeAttendingParticipants = getMaybeAttendingParticipants;
        vm.notAttendingParticipants = getNotAttendingParticipants;
        vm.getAttendingVirtuallyParticipants = getAttendingVirtuallyParticipants;
        vm.getAllAttendingParticipants = getAllAttendingParticipants;

        function toggleExpandCollapse() {
            if(vm.isExpanded) {
                vm.expandCollapseText = 'events.expand';
            }
            else {
                vm.expandCollapseText = 'events.collapse';
            }
            vm.isExpanded = !vm.isExpanded;
        }

        function getAllAttendingParticipants() {
            return getParticipantsByStatuses([attendStatus.Attending, attendStatus.AttendingVirtually]);
        }

        function getAttendingParticipants() {
            return getParticipantsByStatuses([attendStatus.Attending]);
        }

        function getMaybeAttendingParticipants() {
            return getParticipantsByStatuses([attendStatus.MaybeAttending]);
        }

        function getNotAttendingParticipants() {
            return getParticipantsByStatuses([attendStatus.NotAttending]);
        }

        function getAttendingVirtuallyParticipants() {
            return getParticipantsByStatuses([attendStatus.AttendingVirtually]);
        }

        function getParticipantsByStatuses(statuses) {
            return vm.participants.filter(participant => statuses.includes(participant.attendStatus));
        }
    }
})();
