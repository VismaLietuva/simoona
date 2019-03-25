(function () {
    'use strict';

    angular.module('simoonaApp.Picture')
        .filter('backgroundThumb', backgroundThumb);
    
    backgroundThumb.$inject = ['$filter'];

    function backgroundThumb($filter) {
        return function (input, scope) {
            if (input) {
                var url = $filter('thumb')(input, scope);
                return "{'background-image': 'url('+'" + url + "'+')'}";
            }
        }
    }
})();
