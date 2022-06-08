(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('filterPresetService', filterPresetService);

    function filterPresetService() {
        var service = {
            getFiltersByTypeFromResult: getFiltersByTypeFromResult,
            mapFilterPresetTypesToMap: mapFilterPresetTypesToMap
        };

        return service;

        ///////

        function getFiltersByTypeFromResult(result, type) {
            return result
                .find(filter => filter.filterType == type);
        }

        function mapFilterPresetTypesToMap(preset, filterType, loadedFilterTypes) {
            var presetTypes = getFiltersByTypeFromResult(
                preset.filters,
                filterType
            );

            if (!presetTypes) {
                return new Map();
            }

            var filters = Object.values(loadedFilterTypes).find(
                (filter) => filter.filterType == filterType
            ).filters;

            return new Map(
                presetTypes.types.map((type) => [
                    parseInt(type),
                    filters.find((filter) => filter.id == type).name,
                ])
            );
        }
    }
}());
