(function () {
    'use strict';

    var simoonaApp = angular.module('simoonaApp.EmployeeList');

    simoonaApp.factory('employeeListRepository', employeeListRepository);

    employeeListRepository.$inject = ['$resource', 'endPoint'];

    function employeeListRepository($resource, endPoint) {
        var employeeListUrl = endPoint + '/EmployeeList/';

        return {
            getPaged: function (params) {
                return $resource(employeeListUrl + 'GetPaged', '', { 'query': { method: 'GET', isArray: false, params: params } }).query(params).$promise;
            }
        }
    }
})();