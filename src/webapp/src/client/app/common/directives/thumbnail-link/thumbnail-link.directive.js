'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Common');

    simoonaApp.directive('thumbnailLink', function() {
        return {
            restrict: 'AE',
            scope: {
                pictureId: '=',
                sizeReduceTimes: '=?',
                organizationName: '='
            },
            templateUrl: "app/common/directives/thumbnail-link/thumbnail-link.html",
            link: function(scope, element, attrs) {
                if (!scope.sizeReduceTimes)
                    scope.sizeReduceTimes = 1;
            }
        }
    });
})();