(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('wallSharedEventRepository', wallSharedEventRepository);

    wallSharedEventRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function wallSharedEventRepository($resource, endPoint) {
        var eventDetailsUrl = endPoint + '/Event/Details';
        
        var service = {
            getEventDetails: getEventDetails
        };

        return service;

        /////////////////

        function getEventDetails(eventId) {
            return $resource(eventDetailsUrl).get({
                eventId: eventId
            }).$promise;
        }
    }
})();