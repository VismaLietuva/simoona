(function () {
    'use strict';

    angular.module('simoonaApp.RoomType')
        .factory('roomTypeRepository', roomTypeRepository);
    
    roomTypeRepository.$inject = ['$resource', 'endPoint'];

    function roomTypeRepository($resource, endPoint) {
        var url = endPoint + '/RoomType/';

        return {
            get: function (params) {
                return $resource(url + 'Get').get(params).$promise;
            },

            getAll: function () {
                return $resource(url + 'GetAll').query().$promise;
            },

            getByFloor: function (params) {
                return $resource(url + 'GetByFloor').query(params).$promise;
            },

            getPaged: function (params) {
                return $resource(url + 'GetPaged').get(params).$promise;
            },

            getByUserName: function (userName, includeProperties) {
                return $resource(applicationUserUrl + 'GetByUserName', '', { 'query': { method: 'GET', isArray: false } }).query({ userName: userName, includeProperties: includeProperties }).$promise;
            },

            create: function (params) {
                return $resource(url + 'Post').save(params).$promise;
            },

            update: function (params) {
                return $resource(url + 'Put', '', { put: { method: 'PUT' } }).put(params).$promise;
            },

            delete: function (params) {
                return $resource(url + 'Delete').delete({ id: params }).$promise;
            }
        }
    }
})();