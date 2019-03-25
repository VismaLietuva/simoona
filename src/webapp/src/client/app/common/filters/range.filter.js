(function() {
    'use strict';
    angular
        .module('simoonaApp.Common')
        .filter('integerRange', integerRangeFilter);

    function integerRangeFilter() {

        return function(value, minValue, maxValue) {
            if (!isNaN(value)) {
                var returnedValue = parseInt(value);

                if (returnedValue >= maxValue) {
                    return maxValue;
                } else if (returnedValue <= minValue) {
                    return minValue;
                }

                return returnedValue;

            } else {
                return minValue;
            }
        };
    }
})();
