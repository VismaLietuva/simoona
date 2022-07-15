(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceSortMultipleLink', sortMultipleLink);

    function sortMultipleLink() {
        const directive = {
            templateUrl: 'app/common/directives/sort-multiple-link/sort-multiple-link.html',
            restrict: 'E',
            transclude: true,
            replace: true,
            scope: {
                sortDir: '=',
                sortField: '=',
                sortValue: '@',
                position: '=',
                onSort: '='
            },
            controller: sortMultipleLinkController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    function sortMultipleLinkController() {
        /* jshint validthis: true */
        const vm = this;

        vm.sortLinkWasPressed = false;

        vm.sortColumn = sortColumn;

        ////////////

        function sortColumn() {
            if (vm.sortField === vm.sortValue) {
                vm.sortLinkWasPressed = true;
                vm.sortDir = (!vm.sortDir || vm.sortDir === 'asc') ? 'desc' : 'asc';
            } else {
                vm.sortField = vm.sortValue;
                vm.sortDir = 'asc';
            }

            if (vm.sortLinkWasPressed && vm.sortDir === 'asc') {
                vm.sortLinkWasPressed = false;
                vm.sortField = undefined;
                vm.sortDir = undefined;
            } else {
                vm.sortLinkWasPressed = true;
            }

            vm.onSort(vm.sortField, vm.sortDir, vm.position);
        }
    }
})();
