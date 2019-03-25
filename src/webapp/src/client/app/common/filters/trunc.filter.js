(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .filter('trunc', trunc);

    function trunc() {
        return function (input) {
            return Math.trunc(input);
        };
    }
})();