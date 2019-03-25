(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceItemListPagination', itemListPagination);

    function itemListPagination() {
        var directive = {
            templateUrl: 'app/common/directives/item-list-pagination/item-list-pagination.html',
            restrict: 'EA',
            replace: true,
            scope: {
                pageSize: '=',
                totalItemCount: '=',
                onChanged: '=',
                currentPage: '='
            },
            controller: itemListPaginationController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    itemListPaginationController.$inject = [];

    function itemListPaginationController() {
        /* jshint validthis: true */
        var vm = this;

        vm.paging = null;

        vm.changePage = changePage;

        init();

        ////////////

        function init() {
            if (vm.currentPage) {
                vm.paging = vm.currentPage;
            } else {
                vm.paging = 1;
            }
        }

        function changePage() {
            document.body.scrollTop = document.documentElement.scrollTop = 0;
            vm.onChanged();
        }

    }
})();
