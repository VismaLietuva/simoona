(function () {
    'use strict';

    angular
        .module('simoonaApp.Picture')
        .filter('picture', pictureFilter);

    pictureFilter.$inject = [
        '$window',
        'authService',
        'endPoint'
    ];

    function pictureFilter($window, authService, endPoint) {
        return function (input, scope) {
            if (input && authService.identity.isAuthenticated) {
                if (input.endsWith('.gif') && !$window.usingAnimatedGifs) {
                    return endPoint + '/storage/' + authService.identity.organizationName.toLowerCase() + '/' + input;
                }
                return endPoint + '/storage/' + authService.identity.organizationName.toLowerCase() + '/' + input + '?width=1920&height=1080&mode=max';
            }
        };
    }
})();
