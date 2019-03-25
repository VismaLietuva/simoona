(function () {
    'use strict';
    angular
        .module('simoonaApp.Common')
        .filter('arrayToString', arrayToString);

    arrayToString.$inject = ['lodash', '$filter'];

    function arrayToString(lodash, $filter) {
        return function (input, charactersLimit)
        {
            var str = lodash.map(input).join(', ');
            if (charactersLimit)
            {
                return $filter('characters')(str, charactersLimit);
            } else {
                return str;
            }
            
        };
    }

})();