(function () {
    'use strict';
    angular
        .module('simoonaApp.Common')
        .filter('normalizeUrl', normalizeUrl);

    normalizeUrl.$inject = [];

    function normalizeUrl() {
        return function(url) {
            if (url.indexOf('https://') !== -1) {
                return url;
            } else if (url.indexOf('http://') === -1) {
                return 'http://' + url;
            }

            return url;
        };
    }
})();