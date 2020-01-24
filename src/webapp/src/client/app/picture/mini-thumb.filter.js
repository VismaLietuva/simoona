(function () {
    'use strict';

    angular.module('simoonaApp.Picture')
        .filter('miniThumb', miniThumb);

    miniThumb.$inject = ['authService', 'endPoint'];

    function miniThumb(authService, endPoint) {

        return function (input, scope) {

            var identity = authService.identity;

            if (input) {
                return endPoint + "/storage/" + identity.organizationName.toLowerCase() + "/" + input + "?width=40&height=40&mode=max";
            }
            return "/images/MiniThumbPersonPlaceholder.png";
        }
    }
})();
