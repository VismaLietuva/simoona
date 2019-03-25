(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceListPagination', listPagination);

    function listPagination() {
        var directive = {
            templateUrl: 'app/common/directives/list-pagination/list-pagination.html',
            restrict: 'E',
            replace: true,
            scope: {
                pageSize: '=',
                pageCount: '=',
                totalItemCount: '=',
                onChanged: '&',
                currentPage: '='
            },
            controller: listPaginationController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    listPaginationController.$inject = [];

    function listPaginationController() {
        /* jshint validthis: true */
        var vm = this;



        vm.firstPage = firstPage;
        vm.nextPage = nextPage;
        vm.lastPage = lastPage;
        vm.prevPage = prevPage;

        ////////////

        function firstPage() {
            vm.onChanged({
                page: 1
            });
        }

        function nextPage() {
            if (vm.pageCount > vm.currentPage) {
                vm.onChanged({
                    page: vm.currentPage + 1
                });
            }
        }

        function prevPage() {
            if (vm.currentPage > 1) {
                vm.onChanged({
                    page: vm.currentPage - 1
                });
            }
        }

        function lastPage() {
            vm.onChanged({
                page: vm.pageCount
            });
        }
    }
})();
