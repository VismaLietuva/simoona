(function () {
    'use strict';

    angular.module('simoonaApp.Picture')
        .filter('userBackgroundThumb', ['$filter', function ($filter) {
        return function (input, scope) {
            if (input) {
                var url = $filter('userThumb')(input, scope);
                return "{'background-image': 'url('+'"+url+"'+')'}";
            }
            return "{'background-image': 'url('+'/images/PersonPlaceholder.png'+')'}";
        }
    }]);
})();