'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Common');

    simoonaApp.filter('lowerCaseFirstLetter', function() {
        return function(input, scope) {
            if (input != null)
                return input.substring(0, 1).toLowerCase() + input.substring(1);
        }
    });
})();