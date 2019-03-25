(function () {
    'use strict';

    angular.module('simoonaApp.Role')
        .factory('roleRepository', roleRepository);
    
    roleRepository.$inject = ['$resource', 'endPoint'];
    
    function roleRepository($resource, endPoint) {
        var roleUrl = endPoint + '/Role';

        return {
            getPaged: function (params) {
                var url = roleUrl + '/GetPaged';
                params.includeProperties = "Organization";
                return $resource(url, '', { 'query': { method: 'GET', isArray: false, params: params } }).query(params).$promise;
            },
            GetPermissionGroups: function (params) {
                var url = roleUrl + '/GetPermissionGroups';
                return $resource(url, '', { 'query': { method: 'GET', isArray: true } }).query().$promise;
            },
            get: function (params) {
                var url = roleUrl + '/Get';
                return $resource(url).get(params).$promise;
            },
            getRolesForAutoComplete: function (search) {
                var url = roleUrl + '/GetRolesForAutoComplete';
                return $resource(url).query({ search: search }).$promise;
            },
            getUsersForAutoComplete: function (params) {
                var url = roleUrl + '/GetUsersForAutoComplete';
                return $resource(url).query({ s: params }).$promise;
            },
            update: function (params) {
                return $resource(roleUrl + '/Put', '', { put: { method: 'PUT' } }).put(params).$promise;
            },
            create: function (params) {
                return $resource(roleUrl + '/Post').save(params).$promise;
            },
            deleteItem: function (params) {
                return $resource(roleUrl + '/Delete').delete({ roleId: params }).$promise;
            }
        }
    }
})();