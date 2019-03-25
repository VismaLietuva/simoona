(function () {
    'use strict';

    angular.module('simoonaApp.Office')
        .factory('officeRepository', officeRepository);
    
    officeRepository.$inject = ['$resource', 'endPoint'];

    function officeRepository($resource, endPoint) {
        var officeUrl = endPoint + '/Office/';
        var officeMapUrl = endPoint + '/Map/';
        var offices = [];

        return {
            get: function (officeId) {
                return $resource(officeUrl + 'Get').get({ id: officeId }).$promise;
            },
            getDefault: function () {
                return $resource(officeUrl + 'GetDefault').get().$promise;
            },
            getAll: function (params) {
                return $resource(officeUrl + 'GetAll', '', { 'query': { method: 'GET', isArray: true, params: params } }).query(params).$promise;
            },
            getPaged: function (params) {
                return $resource(officeUrl + 'GetPaged', '', { 'query': { method: 'GET', isArray: false, params: params } }).query(params).$promise;
            },
            setOffices: function (officeToSet) {
                offices = officeToSet;
            },
            getOffice: function (params) {
                //TODO create office object pass from controller
                //It will save time, because you will not need to call server
                return $resource(officeUrl + 'Get').get({ id: params }).$promise;
            },
       
            create: function(office) {
                return $resource(officeUrl + 'Post', '', { post: { method: 'POST' } }).post(office).$promise;
            },
            update: function (office) {
                return $resource(officeUrl + 'Put', '', { Put: { method: 'PUT' } }).Put(office).$promise;
            },
            delete: function (id) {
                return $resource(officeUrl + 'Delete/' + id).delete(id).$promise;
            },
            getUsersEmailsByFloor: function(floorId) {
                return $resource(officeMapUrl + 'GetUsersEmailsByFloor', '', { 'query': { method: 'GET', isArray: true, cache: true} }).query({ floorId: floorId }).$promise;
            },
            getUsersEmailsByOffice: function(officeId) {
                return $resource(officeMapUrl + 'GetUsersEmailsByOffice', '', { 'query': { method: 'GET', isArray: true, cache: true} }).query({ officeId: officeId }).$promise;
            },
            getUsersEmailsByRoom: function(roomId) {
                return $resource(officeMapUrl + 'GetUsersEmailsByRoom', '', { 'query': { method: 'GET', isArray: true, cache: true} }).query({ roomId: roomId }).$promise;
            }
        }
    }
})();