(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .filter('imageThumb', imageThumb);

    imageThumb.$inject = [
        '$window',
        'endPoint', 
        'authService'
    ];

    function imageThumb($window, endPoint, authService) {
        return function (input, size, mode) {
            var parameters = '';
            if (input && size && mode) {
                parameters = '?width=' + size.w + '&height=' + size.h + '&mode=' + mode;
                if (input.endsWith('.gif') && !$window.usingAnimatedGifs) {
                    return endPoint + '/storage/' + authService.identity.organizationName.toLowerCase() + '/' + input;
                }
                return endPoint + '/storage/' + authService.identity.organizationName.toLowerCase() + '/' + input + parameters;
            }

            if(size && mode) {
                parameters = '?width=' + size.w + '&height=' + size.h + '&mode=' + mode;
            }

            return 'images/wall-default.png' + parameters;
        };
    }
})();
