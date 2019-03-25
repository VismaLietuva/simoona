(function () {
    'use strict';

    angular.module('simoonaApp.Picture')
        .filter('userPicture', ["$filter", function ($filter) {
        return function (input, scope) {
            if (input) {
                return $filter('picture')(input, scope);
            }

            return "/images/PersonPlaceholder.png";
        }
    }]);
})();
