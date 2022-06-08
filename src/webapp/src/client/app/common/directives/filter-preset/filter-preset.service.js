(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('filterPresetService', filterPresetService);

    function filterPresetService() {
        var service = {
            getFiltersByTypeFromResult: getFiltersByTypeFromResult
        };

        return service;

        ///////

        function getFiltersByTypeFromResult(result, type) {
            return result
                .find(filter => filter.filterType == type);
        }
    }
}());
