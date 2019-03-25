(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.EventTypes')
        .factory('eventTypesRepository', eventTypesRepository);

    eventTypesRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function eventTypesRepository($resource, endPoint) {
        var eventTypesUrl = endPoint + '/EventType/';

        var service = {
            getEventTypes: getEventTypes,
            getEventType: getEventType,
            createEventType: createEventType,
            updateEventType: updateEventType,
            deleteEventType: deleteEventType
        };
        return service;

        ///////////

        function getEventTypes() {
            return $resource(eventTypesUrl + 'Types').query().$promise;
        }

        function getEventType(id) {
            return $resource(eventTypesUrl + 'Get').get({ id: id }).$promise;
        }

        function createEventType(eventType) {
            return $resource(eventTypesUrl + 'Create').save(eventType).$promise;
        }

        function updateEventType(eventType) {
            return $resource(eventTypesUrl + 'Update', '', {
                put: {
                    method: 'PUT'
                }
            }).put(eventType).$promise;
        }

        function deleteEventType(id) {
            return $resource(eventTypesUrl + 'Delete').delete({ id: id }).$promise;
        }
    }
})();
