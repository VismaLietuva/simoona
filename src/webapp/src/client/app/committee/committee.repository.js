(function () {
    'use strict';

    var simoonaApp = angular.module('simoonaApp.Committee');

    simoonaApp.factory('committeeRepository',   committeeRepository);

    committeeRepository.$inject = ['$resource', 'endPoint'];

    function committeeRepository($resource, endPoint) {
        var committeesUrl = endPoint + '/Committees/';
        return {
            getAll: function (params) {
                return $resource(committeesUrl + 'GetAll', '', { 'query': { method: 'GET', isArray: true, params: params } }).query(params).$promise;
            },
            getPaged: function (params) {
                return $resource(committeesUrl + 'GetPaged', '', { 'query': { method: 'GET', isArray: false, params: params } }).query(params).$promise;
            },
            getUsers: function (params) {
                return $resource(endPoint + '/ApplicationUser/GetForAutoComplete').query({ s: params }).$promise;
            },
            post: function (model) {
                return $resource(committeesUrl + 'Post').save(model).$promise;
            },
            put: function (model) {
                return $resource(committeesUrl + 'Put', '', { Put: { method: 'PUT' } }).Put(model).$promise;
            },
            deleteItem: function (params) {
                return $resource(committeesUrl + 'Delete').delete({ Id: params }).$promise;
            },
            getKudosCommittee: function () {
                return $resource(committeesUrl + 'KudosCommittee').get().$promise;
            },
            getKudosCommitteeId : function (){
                return $resource(committeesUrl + 'KudosCommitteeId').get().$promise;
            },
            postSuggestion: function (model) {
                return $resource(committeesUrl + 'PostSuggestion').save(model).$promise;
            },
            deleteSuggestion: function (comitteId, suggestionId) {
                return $resource(committeesUrl + 'DeleteSuggestion').delete({ ComitteeId: comitteId, SuggestionId: suggestionId }).$promise;
            },
            getSuggestions: function (committeeId) {
                return $resource(committeesUrl + 'GetSuggestions').query( {id: committeeId}).$promise;
            }

        }

    }
})();