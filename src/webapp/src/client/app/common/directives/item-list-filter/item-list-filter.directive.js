(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceItemListFilter', itemListFilter);

    function itemListFilter() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/item-list-filter/item-list-filter.html',
            scope: {
                filterValue: '=',
                onFiltering: '=',
                onFilterReset: '=',
                defaultLayout: '=?'
            },
            controller: itemListFilterController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    itemListFilterController.$inject = [];

    function itemListFilterController() {
        /* jshint validthis: true */
        var vm = this;
        vm.filterValue = vm.filterValue || '';
        vm.executeFilter = executeFilter;
        vm.filterInputReset = filterInputReset;

        ////////////

        function executeFilter() {
                vm.onFiltering(vm.filterValue);
        }

        function filterInputReset() {
            if (vm.onFilterReset) {
                vm.onFilterReset();
                return;
            }

            vm.filterValue = '';
            vm.onFiltering(vm.filterValue);
        }

    }

})();
