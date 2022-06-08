(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('filterPresetRepository', filterPresetRepository);


    filterPresetRepository.$inject = [
        '$resource',
        'endPoint',
    ];

    function filterPresetRepository($resource, endPoint) {
        var filterPresetUrl = `${endPoint}/FilterPreset`;

        var service = {
            getPresetsForPage: getPresetsForPage,
            getFilters: getFilters,
            updatePresets: updatePresets
        };

        return service;

        function updatePresets(presets) {
            return $resource(`${filterPresetUrl}/Update`, {}, {
                post: {
                    withCredentials: true,
                    method: 'POST'
                }
            }).post({
                presetsToUpdate: presets.presetsToUpdate,
                presetsToAdd: presets.presetsToAdd,
                presetsToRemove: presets.presetsToRemove
            }).$promise;
        }

        function getPresetsForPage(filterPageType) {
            return $resource(`${filterPresetUrl}/GetPresetsForPage`, '', {
                'GET': {
                    method: 'GET',
                    isArray: true
                }
            })
                .query({
                    pageType: filterPageType
                })
                .$promise;
        }

        function getFilters(filterTypes) {
            return $resource(`${filterPresetUrl}/GetFilters`, '', {
                'GET': {
                    method: 'GET',
                    isArray: true
                }
            })
                .query({
                    filterTypes: filterTypes
                })
                .$promise;
        }
    }
})();
