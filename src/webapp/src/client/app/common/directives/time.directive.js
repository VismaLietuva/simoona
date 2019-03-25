(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .directive('time', time);

    time.$inject = [];

    /**
     * @name time
     *
     * @description
     *      Checks if provided value is time 'hh:mm' or 'h:mm'. Seconds are optional and will possibly be trimmed when displaying.
     *
     * @usage
     *      <input ng-model="prop" time />
     * 
     * @param {String} prop - property of type String
     *
     */
    function time() {
        var link = function (scope, element, attrs, ngModel) {
            if (!ngModel) return;
            var regex = /^ *((0?|1)[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])? *$/;

            var validator = function (value) {
                ngModel.$setValidity('time', value ? regex.test(value) : true);
                return value;
            };

            ngModel.$parsers.unshift(validator);
            ngModel.$formatters.unshift(validator);
        }

        return {
            require: 'ngModel',
            restrict: 'A',
            link: link
        };
    }
})();