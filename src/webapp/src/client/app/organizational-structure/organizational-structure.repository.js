(function () {
    'use strict';

    angular
        .module('simoonaApp.OrganizationalStructure')
        .factory('organizationalStructureRepository', organizationalStructureRepository);

    organizationalStructureRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function organizationalStructureRepository($resource, endPoint) {
        var organizationalStructureUrl = endPoint + '/OrganizationalStructure/';

        var service = {
            getStructure: getStructure
        };

        return service;

        function getStructure() {
            return $resource(organizationalStructureUrl + 'GetOrganizationalStructure').get().$promise;
        }
    }
})();
