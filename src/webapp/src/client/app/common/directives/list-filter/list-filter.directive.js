(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceListFilter', listFilter);

    function listFilter() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/list-filter/list-filter.html',
            scope: {
                onFiltering: '&',
                minCharacters: '='
            },
            controller: listFilterController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    listFilterController.$inject = [];

    function listFilterController() {
        /* jshint validthis: true */
        var vm = this;

        vm.filterValue = '';
        vm.minCharacters = !!vm.minCharacters ? vm.minCharacters : 2;
        vm.onSearch = onSearch;

        function onSearch(searchString) {
            if (searchString.length === 0 || searchString.length >= vm.minCharacters) {
                vm.onFiltering({
                    search: searchString
                });
            }
        }
    }
})();
