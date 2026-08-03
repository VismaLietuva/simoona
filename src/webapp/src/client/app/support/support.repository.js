(function () {
    'use strict';

    angular
        .module('simoonaApp.Support')
        .factory('supportRepository', supportRepository);

    supportRepository.$inject = [
        '$resource',
        '$http',
        'endPoint'
    ];

    function supportRepository($resource, $http, endPoint) {

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

        // SubmitTicket takes multipart/form-data since it gained an optional
        // screenshot, so this posts FormData rather than JSON. This UI does not
        // offer an image picker; the new UI does. Authorization/Organization
        // headers come from authInterceptor.
        function submitTicket(ticket) {
            var formData = new FormData();
            formData.append('Subject', ticket.subject);
            formData.append('Message', ticket.message);
            formData.append('Type', ticket.type);

            return $http.post(supportUrl + 'SubmitTicket', formData, {
                transformRequest: angular.identity,
                headers: {
                    'Content-Type': undefined
                }
            });
        }
    }
})();