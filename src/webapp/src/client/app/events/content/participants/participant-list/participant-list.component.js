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
        vm.AttendingParticipants = AttendingParticipants;
        vm.MaybeAttendingParticipants = MaybeAttendingParticipants;
        vm.NotAttendingParticipants = NotAttendingParticipants;

        function toggleExpandCollapse() {
            if(vm.isExpanded) {
                vm.expandCollapseText = 'events.expand';
            }
            else {
                vm.expandCollapseText = 'events.collapse';
            }
            vm.isExpanded = !vm.isExpanded;
        }

        function AttendingParticipants() {
            return getParticipantsByStatus(attendStatus.Attending);
        }

        function MaybeAttendingParticipants() {
            return getParticipantsByStatus(attendStatus.MaybeAttending);
        }

        function NotAttendingParticipants() {
            return getParticipantsByStatus(attendStatus.NotAttending);
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
