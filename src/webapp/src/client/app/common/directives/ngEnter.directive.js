'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Common');

    simoonaApp.directive('ngEnter', function() {
        return {
            restrict: 'A',
            transclude: true,
            link: function(scope, element, attrs) {
                element.bind("keydown keypress", function(event) {
                    if (event.which === 13) {
                        scope.$apply(function() {
                            scope.$eval(attrs.ngEnter);
                        });

                        event.preventDefault();
                    }
                });
            }
        };
    });
})();