(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .filter('collapseComments', collapseComments);

    function collapseComments() {

        return function (arr, isSliced) {
            if (isSliced) {
                return (arr || []).slice(-2);
            } else {
                return (arr || []);
            }
        };
    }
})();