(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('genericSrv', ['$resource', function ($resource) {

            var genericSrv = function (url) {

                var get = function (params) {
                    return $resource(url + 'Get').get(params).$promise;
                };

                var getAll = function () {
                    return $resource(url + 'GetAll').query().$promise;
                };

                var getPaged = function (params) {
                    return $resource(url + 'GetPaged').get(params).$promise;
                };

                var genericQuery = function (methodName) {
                    return $resource(url + methodName).query().$promise;
                };

                var create = function (params) {
                    return $resource(url + 'Post', '', { post: { method: 'POST' } }).post(params).$promise;
                };

                var update = function (params) {
                    return $resource(url + 'Put', '', { put: { method: 'PUT' } }).put(params).$promise;
                };

                var del = function (params) {
                    return $resource(url + 'Delete').delete({ id: params }).$promise;
                };
                return {
                    get: get,
                    getAll: getAll,
                    getPaged: getPaged,
                    genericQuery: genericQuery,
                    create: create,
                    update: update,
                    deleteItem: del
                }
            };
            return genericSrv;
        }]);
})();