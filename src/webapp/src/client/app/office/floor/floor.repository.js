(function () {
    'use strict';

    angular.module('simoonaApp.Office.Floor')
        .factory('floorRepository', floorRepository);

    floorRepository.$inject = ['$resource', 'endPoint'];

    function floorRepository($resource, endPoint) {
        var floorUrl = endPoint + '/Floor/';

        return {
            get: function(params, prop) {
                params.properties = prop;
                return $resource(floorUrl + 'Get').get(params).$promise;
            },

            getPaged: function (params) {
                return $resource(floorUrl + 'GetAllFloors').get(params).$promise;
            },

            getByOffice: function(officeId) {
                return $resource(floorUrl + 'GetByOffice', '', { 'query': { method: 'GET', isArray: true } }).query({ officeId: officeId }).$promise;
            },
            getByRoom: function(roomId) {
                return $resource(floorUrl + 'GetByRoom').get(roomId).$promise;
            },
            create: function(floor) {
                return $resource(floorUrl + 'Post').save(floor).$promise;
            },
            update: function(floor) {
                return $resource(floorUrl + 'Put', {}, { put: { method: 'PUT' } }).put(floor).$promise;
            },
            delete: function(id) {
                return $resource(floorUrl + 'Delete').delete({ id: id }).$promise;
            }
        }
    }
})();