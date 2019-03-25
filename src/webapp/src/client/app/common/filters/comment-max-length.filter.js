'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Common');

    simoonaApp.filter('commentMaxLength', function() {
        return function(input, scope) {
            if (input != null && input.length > 10)
                return input.substring(0, 10) + "...";
            else {
                return input;
            }
        }
    });
})();