(function () {
    'use strict';

    angular
        .module('simoonaApp.Support')
        .factory('supportRepository', supportRepository);

    supportRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function supportRepository($resource, endPoint) {
        
        var supportUrl = endPoint + '/Support/';

        var service = {
            getTypes: getTypes,
            submitTicket: submitTicket
        };
        return service;

         /////////

        function getTypes() {
            return $resource(supportUrl + 'GetSupportTypes').query().$promise;
        }

        function submitTicket(ticket) {
            return $resource(supportUrl + 'SubmitTicket').save(ticket).$promise;
        }
    }
})();