(function () {
    'use strict';

    angular
        .module('simoonaApp.Recommendation')
        .factory('recommendationRepository', recommendationRepository);

    recommendationRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function recommendationRepository($resource, endPoint) {
        
        var supportUrl = endPoint + '/Recommendation/';

        var service = {
            submitTicket: submitTicket
        };
        return service;

         /////////

        function submitTicket(ticket) {
            return $resource(supportUrl + 'SubmitTicket').save(ticket).$promise;
        }
    }
})();