(function () {
    'use strict';

    angular.module('simoonaApp.Profile')
        .filter('dateConvertFilter', dateConvertFilter);
    
    dateConvertFilter.$inject = [];

    function dateConvertFilter() {
        return function (dateString) {
            //var d = Date.parse(unformattedDate);
            if (dateString == null)
                return null;
            return new Date(dateString);
        }
    }
})();
