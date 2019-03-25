(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceSortLink', sortLink);

    function sortLink() {
        var directive = {
            templateUrl: 'app/common/directives/sort-link/sort-link.html',
            restrict: 'E',
            transclude: true,
            replace: true,
            scope: {
                sortDir: '=',
                sortField: '=',
                sortValue: '@',
                onSort: '='
            },
            controller: sortLinkController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    function sortLinkController() {
        /* jshint validthis: true */
        var vm = this;

        vm.sortColumn = sortColumn;

        ////////////

        function sortColumn() {
            if (vm.sortField === vm.sortValue) {
                vm.sortDir = vm.sortDir === 'asc' ? 'desc' : 'asc';
            } else {
                vm.sortField = vm.sortValue;
                vm.sortDir = 'asc';
            }

            vm.onSort(vm.sortField, vm.sortDir);
        }
    }
})();
