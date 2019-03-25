(function () {
    'use strict';

    angular
        .module('simoonaApp.ServiceRequest')
        .directive('serviceRequestTable', serviceRequestTable);

    function serviceRequestTable() {
        var directive = {
            templateUrl: 'app/service-request/service-request-table.html',
            restrict: 'E',
            link: linkFunc
        };
        return directive;

        function linkFunc(scope) {

        }
    }
})();
