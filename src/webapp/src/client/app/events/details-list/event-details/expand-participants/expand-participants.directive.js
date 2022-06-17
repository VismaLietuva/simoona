(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .directive('aceExpandParticipants', aceExpandParticipants);

    function aceExpandParticipants() {
        var directive = {
            templateUrl: 'app/events/details-list/event-details/expand-participants/expand-participants.html',
            restrict: 'E',
            replace: true,
            scope: {
                visitedEvents: '='
            },
            controller: expandParticipantsController,
            controllerAs: 'vm',
            bindToController: true,
        }

        return directive;
    }

    expandParticipantsController.$inject = [ 'lodash' ];

    function expandParticipantsController(lodash) {
        var vm = this;

        vm.filter = {
            sortOrder: 'desc',
            sortByColumnName: 'name'
        };

        vm.onSort = onSort;

        function onSort(sortBy, sortOrder) {
            vm.visitedEvents = lodash.orderBy(vm.visitedEvents, [sortBy], [sortOrder]);
        }
    }
})();
