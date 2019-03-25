(function () {
    'use strict';

    angular.module('simoonaApp.Office.Floor.Room')
        .factory('roomRepository', roomRepository);
    
    roomRepository.$inject = ['$resource', 'endPoint'];

    function roomRepository($resource, endPoint) {
        var url = endPoint + '/Room/';

        return {
            getPaged: function (params) {
                return $resource(url + 'GetAllRoomsByFloor', '', { 'query': { method: 'GET', isArray: false } }).query(params).$promise;
            },

            getAll: function(params) {
                return $resource(url + 'GetForAutocomplete').query(params).$promise;
            },

            get: function(params) {
                return $resource(url + 'Get', '', { 'get': { method: 'GET', isArray: false } }).get(params).$promise;
            },

            getByFloor: function(floorId, includeProperties, includeWorkingRooms, includeNotWorkingRooms) {
                return $resource(url + 'GetByFloor', '', { 'get': { method: 'GET', isArray: true } }).get({ floorId: floorId, includeProperties: includeProperties, includeWorkingRooms: includeWorkingRooms, includeNotWorkingRooms: includeNotWorkingRooms }).$promise;
            },

            create: function(params) {
                return $resource(url + 'Post').save(params).$promise;
            },

            update: function(room) {
                return $resource(url + 'Put', '', { put: { method: 'PUT' } }).put(room).$promise;
            },

            delete: function(id) {
                return $resource(url + 'Delete').delete({ id: id }).$promise;
            }
        }
    }
})();