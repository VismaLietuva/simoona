(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.KudosTypes')
        .factory('kudosTypesRepository', kudosTypesRepository);

    kudosTypesRepository.$inject = ['$resource', 'endPoint'];

    function kudosTypesRepository($resource, endPoint) {
        var kudosUrl = endPoint + '/Kudos/';

        var service = {
            getKudosTypes: list,
            createKudosType: create,
            updateKudosType: update,
            getKudosType: get,
            removeType: removeType
        }

        return service;
        
        ///////////

        function list() {
            return $resource(kudosUrl + 'GetKudosTypes').query().$promise;
        }

        function create(type) {
            return $resource(kudosUrl + 'CreateType').save(type).$promise;
        }

        function update(type) {
            return $resource(kudosUrl + 'EditType', '', {
                put: {
                    method: 'PUT'
                }
            }).put(type).$promise;
        }

        function get(id) {
            return $resource(kudosUrl + 'EditType').get({ id: id }).$promise;
        }

        function removeType(id) {
            return $resource(kudosUrl + 'RemoveType/:id', { id: id }, {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }
    }
})();