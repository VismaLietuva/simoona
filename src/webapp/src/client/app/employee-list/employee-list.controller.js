(function () {
    'use strict';
    angular
        .module('simoonaApp.EmployeeList')
        .controller('EmployeeListController', EmployeeListController);

    EmployeeListController.$inject = [
        '$scope',
        '$stateParams',
        'employeeListRepository',
        'authService',
        'notifySrv',
        '$rootScope',
        '$translate',
        'employeeList',
        '$timeout'
    ];

    function EmployeeListController($scope, $stateParams, employeeListRepository,
        authService, notifySrv, $rootScope, $translate, employeeList, $timeout) {

        $scope.pagedEmployeeList = employeeList;
        $rootScope.pageTitle = 'common.employeeList';

        $scope.isAdmin = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);

        $scope.filter = {
            page: 1,
            sortOrder: 'asc',
            sortBy: 'LastName',
            search: ''
        };

        $scope.getEmployeeList = function () {
            employeeListRepository.getPaged($scope.filter).then(function (getPagedResponse) {
                $scope.pagedEmployeeList = getPagedResponse;
            });
        };

        $scope.searchReset = function () {
            $scope.filter.search = '';
            $scope.filter.sortOrder = 'asc';
            $scope.filter.sortBy = 'LastName';
            $scope.filter.page = 1;
            $scope.getEmployeeList();
        };

        $scope.onSort = function (sortBy, sortOrder) {
            $scope.filter.sortOrder = sortOrder;
            $scope.filter.sortBy = sortBy;
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
