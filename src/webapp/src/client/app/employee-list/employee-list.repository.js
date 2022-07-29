(function () {
    'use strict';

    var simoonaApp = angular.module('simoonaApp.EmployeeList');

    simoonaApp.factory('employeeListRepository', employeeListRepository);

    employeeListRepository.$inject = ['$resource', 'endPoint'];

    function employeeListRepository($resource, endPoint) {
        var employeeListUrl = endPoint + '/Employees/';

        return {
            getPaged: function (params) {
                return $resource(employeeListUrl, '', { 'query': { method: 'GET', isArray: false, params: params } }).query(params).$promise;
            }
        }
    }
})();
