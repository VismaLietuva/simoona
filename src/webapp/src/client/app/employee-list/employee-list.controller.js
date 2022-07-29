(function () {
    'use strict';
    angular
        .module('simoonaApp.EmployeeList')
        .controller('EmployeeListController', EmployeeListController);

    EmployeeListController.$inject = [
        '$scope',
        'employeeListRepository',
        'authService',
        '$rootScope',
        'employeeList',
        'sortMultipleLinkService',
    ];

    function EmployeeListController(
        $scope,
        employeeListRepository,
        authService,
        $rootScope,
        employeeList,
        sortMultipleLinkService
    ) {
        $scope.pagedEmployeeList = employeeList;
        $rootScope.pageTitle = 'common.employeeList';

        $scope.isAdmin = authService.hasPermissions([
            'APPLICATIONUSER_ADMINISTRATION',
        ]);

        $scope.hasBlacklistPermission = authService.hasPermissions([
            'BLACKLIST_BASIC'
        ]);

        $scope.filter = {
            page: 1,
            search: '',
            sortValues: sortMultipleLinkService.getMultipleSort(5),
            showOnlyBlacklisted: false
        };

        $scope.getEmployeeList = function () {
            employeeListRepository
                .getPaged({
                    page: $scope.filter.page,
                    search: $scope.filter.search,
                    sortByProperties: $scope.filter.sortValues.getSortString(),
                    showOnlyBlacklisted: $scope.filter.showOnlyBlacklisted
                })
                .then(function (getPagedResponse) {
                    $scope.pagedEmployeeList = getPagedResponse;
                });
        };

        $scope.searchReset = function () {
            $scope.filter.search = '';
            $scope.filter.page = 1;
            $scope.getEmployeeList();
        };

        $scope.onSort = function (sortBy, sortOrder, position) {
            $scope.filter.sortValues.setSortValues(sortBy, sortOrder, position);
            $scope.filter.page = 1;
            $scope.getEmployeeList();
        };

        $scope.onSearch = function (search) {
            $scope.filter.page = 1;
            $scope.filter.search = search;
            $scope.getEmployeeList();
        };

        $scope.changedPage = function () {
            $scope.getEmployeeList();
        };
    }
})();
