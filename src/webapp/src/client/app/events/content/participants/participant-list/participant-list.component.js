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
        vm.getAttendingVirtuallyParticipant = getAttendingVirtuallyParticipants;

        function toggleExpandCollapse() {
            if(vm.isExpanded) {
                vm.expandCollapseText = 'events.expand';
            }
            else {
                vm.expandCollapseText = 'events.collapse';
            }
            vm.isExpanded = !vm.isExpanded;
        }

        function getAttendingParticipants() {
            return getParticipantsByStatus(attendStatus.Attending);
        }

        function getMaybeAttendingParticipants() {
            return getParticipantsByStatus(attendStatus.MaybeAttending);
        }

        function getNotAttendingParticipants() {
            return getParticipantsByStatus(attendStatus.NotAttending);
        }

        function getAttendingVirtuallyParticipants() {
            return getParticipantsByStatus(attendStatus.AttendingVirtually);
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
