'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Common');

    simoonaApp.filter("format", function () {
        return function (input) {
            if (input) {
                var args = arguments;
                return input.replace(/\{(\d+)\}/g, function (match, capture) {
                    return args[1 * parseInt(capture) + 1];
                });
            }
        };
    });
})();