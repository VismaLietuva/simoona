(function () {
    'use strict';

    angular.module('simoonaApp.Picture')
        .filter('userThumb', ["$filter", function ($filter) {
        return function (input, scope) {
            if (input) {
                return $filter('thumb')(input, scope);
            }

            return "/images/PersonPlaceholder.png";
        }
    }]);
})();
