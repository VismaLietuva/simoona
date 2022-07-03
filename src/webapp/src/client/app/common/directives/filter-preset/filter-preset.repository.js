(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('filterPresetRepository', filterPresetRepository);

    filterPresetRepository.$inject = ['$resource', 'endPoint'];

    function filterPresetRepository($resource, endPoint) {
        var filterPresetUrl = `${endPoint}/FilterPreset`;

        var service = {
            getPresetsForPage: getPresetsForPage,
            getFilters: getFilters,
            updatePresets: updatePresets,
        };

        return service;

        function updatePresets(presets, pageType) {
            return $resource(
                `${filterPresetUrl}/Update`,
                {},
                {
                    post: {
                        withCredentials: true,
                        method: 'POST',
                    },
                }
            ).post({
                pageType: pageType,
                presetsToUpdate: presets.presetsToUpdate,
                presetsToCreate: presets.presetsToCreate,
                presetsToDelete: presets.presetsToDelete,
            }).$promise;
        }

        function getPresetsForPage(filterPageType) {
            return $resource(`${filterPresetUrl}/GetPresetsForPage`, '', {
                GET: {
                    method: 'GET',
                    isArray: true,
                },
            }).query({
                pageType: filterPageType,
            }).$promise;
        }

        function getFilters(filterTypes) {
            return $resource(`${filterPresetUrl}/GetFilters`, '', {
                GET: {
                    method: 'GET',
                    isArray: true,
                },
            }).query({
                filterTypes: filterTypes,
            }).$promise;
        }
    }
})();
