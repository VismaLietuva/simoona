(function () {
    'use strict';

    angular.module('simoonaApp.ServiceRequest')
        .directive('serviceRequestFilters', serviceRequestFilters);

    serviceRequestFilters.$inject = [];

    function serviceRequestFilters() {

        return {
            templateUrl: 'app/service-request/service-request-filters.html',
            restrict: 'E'
        }
    }
})();