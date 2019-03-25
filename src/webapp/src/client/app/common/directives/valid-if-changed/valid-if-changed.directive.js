(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('validIfChanged', validIfChanged);

    function validIfChanged() {
        var directive = {
            restrict: 'A',
            require: 'ngModel',
            scope: {
                validIfChanged: '@'
            },
            link: function(scope, element, attributes, ngModelCtrl) {
                ngModelCtrl.$validators.validIfChanged = function(modelValue) {
                    return modelValue !== scope.validIfChanged;
                };
            }
        };

        return directive;
    }
})();
