(function() {
    'use strict';

    angular.module('simoonaApp.Common')
        .filter('convertToCurrentTimezone', convertToCurrentTimezoneFilter);

    convertToCurrentTimezoneFilter.$inject = [];

    function convertToCurrentTimezoneFilter() {

        return function(date) {
            date = date + 'Z'; //Appending z to get correct UTC datetime string
            return new Date(date);
        }
    }
})();
