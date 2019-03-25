(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .filter('participantThumb', participantThumb);

    participantThumb.$inject = [
        '$window',
        'endPoint',
        'authService'
    ];

    function participantThumb($window, endPoint, authService) {
        return function (input, options) {

            if (input && !!options.width && !!options.height && !!options.mode) {
                var parameters = '?width=' + options.width + '&height=' + options.height + '&mode=' + options.mode;
                if (input.endsWith('.gif') && !$window.usingAnimatedGifs) {
                    return endPoint + '/storage/' + authService.identity.organizationName.toLowerCase() + '/' + input;
                }
                return endPoint + '/storage/' + authService.identity.organizationName.toLowerCase() + '/' + input + parameters;
            } else {
                return '/images/participantThumb.png';
            }
        };
    }
})();
