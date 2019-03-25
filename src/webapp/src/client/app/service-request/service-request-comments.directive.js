(function () {
    'use strict';

    angular.module('simoonaApp.ServiceRequest')
        .directive('serviceRequestComments', serviceRequestComments);

    serviceRequestComments.$inject = [];

    function serviceRequestComments() {

        return {
            templateUrl: 'app/service-request/service-request-comments.html',
            restrict: 'E'
        }
    }
})();