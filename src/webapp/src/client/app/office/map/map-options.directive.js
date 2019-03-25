'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Map');

    simoonaApp.directive('mapOptions', function() {
        return {
            controller: 'mapOptionsController',
            restrict: 'AE',
            scope: false,
            templateUrl: 'app/office/map/map-options.html'
        };
    });
})();