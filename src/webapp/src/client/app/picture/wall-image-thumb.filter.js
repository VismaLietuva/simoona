(function () {
    'use strict';

    angular
        .module('simoonaApp.Picture')
        .filter('wallImageThumb', ["authService", "endPoint", "$window", 
        function (authService, endPoint, $window) {
            return function (input, scope) {
                var identity = authService.identity;
                if (input && identity.isAuthenticated) {
                    if (input.endsWith('.gif') && !$window.usingAnimatedGifs) {
                        return endPoint + "/storage/" + identity.organizationName.toLowerCase() + "/" + input;
                    }
                    return endPoint + "/storage/" + identity.organizationName.toLowerCase() + "/" + input + "?width=425&height=300&mode=max";
                }
            }
        }]);
})();
