(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .directive('aceJoinedParticipantCount', JoinedParticipantCount);

    function JoinedParticipantCount() {
        var directive = {
            templateUrl: 'app/events/shared/joined-participant-count/joined-participant-count.html',
            restrict: 'E',
            replace: true,
            scope: {
                event: '='
            },
            controller: Controller,
            controllerAs: 'vm',
            bindToController: true,
        }

        return directive;
    }

    Controller.$inject = [
        '$scope',
        'eventService'
    ]

    function Controller($scope, eventService) {
        var vm = this;

        vm.showPopover = false;
        vm.isOnlyVirtualParticipants = false;
        vm.colors = {
            both: false,
            normal: false,
            virtual: false
        };

        vm.getTotalGoingParticipantCount = getTotalGoingParticipantCount;
        vm.getTotalMaxParticipantCount = getTotalMaxParticipantCount;
        vm.hasMultipleJoinTypes = hasMultipleJoinTypes;

        init();

        function init() {
            managePopoverState();
        }

        function managePopoverState() {
            // Need to watch the value since event could be loaded after this component is initialized
            $scope.$watch('vm.event', function () {
                vm.showPopover = hasMultipleJoinTypes();
                vm.isOnlyVirtualParticipants = !vm.showPopover && vm.event.maxVirtualParticipants > 0;
            });
        }

        function getTotalGoingParticipantCount() {
            return eventService.getTotalGoingParticipantCount(vm.event);
        }

        function hasMultipleJoinTypes() {
            return vm.event.maxParticipants !== 0 && vm.event.maxVirtualParticipants !== 0;
        }

        function getTotalMaxParticipantCount() {
            return eventService.getTotalMaxParticipantCount(vm.event);
        }
    }
})();
