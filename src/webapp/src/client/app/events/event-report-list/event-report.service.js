(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .factory('eventReportService', eventReportService);


    eventReportService.$inject = [
        'sortMultipleLinkService'
    ];

    function eventReportService(sortMultipleLinkService) {
        function eventReportFilter(pageType, filterTypes, columnCount) {
            function constructObjectWithFilterNames(func, response) {
                var property = {};

                filterTypes.forEach((filter) => {
                    property = Object.assign({
                        [filter.name]: func(response, filter)
                    }, property);
                });

                return property;
            }

            function findFilterByType(result, filter) {
                return result.find((f) => f.filterType == filter.type);
            }

            function mapFilterPresetTypesToMap(preset, filter) {
                var presetTypes = findFilterByType(
                    preset.filters,
                    filter
                );

                if (!presetTypes) {
                    return new Map();
                }

                var filters = Object.values(this.filterTypes).find(
                    (f) => f.filterType == filter.type
                ).filters;

                return new Map(
                    presetTypes.types.map((type) => [
                        parseInt(type),
                        filters.find((f) => f.id == type).name,
                    ])
                );
            }

            filterTypes = filterTypes.flat();

            this.dropdown = constructObjectWithFilterNames(() => new Map());
            this.filterTypes = constructObjectWithFilterNames(() => undefined);
            this.appliedFilters = constructObjectWithFilterNames(() => undefined);

            this.pageType = pageType;

            this.appliedFilters.sortValues = sortMultipleLinkService.getMultipleSort(columnCount);

            this.setFilterTypes = function (response) {
                this.filterTypes = constructObjectWithFilterNames(
                    findFilterByType,
                    response
                );
            };

            this.setSortValues = function (sortBy, sortOrder, position) {
                this.appliedFilters.sortValues.setSortValues(sortBy, sortOrder, position);
            }

            this.updateAppliedFilter = function (filter, filterName) {
                this.dropdown[filterName] = new Map(filter);
                this.appliedFilters[filterName] = [...this.dropdown[filterName]]
                    .map(f => f[0]);
            }

            this.getSortString = function () {
                return this.appliedFilters.sortValues.getSortString();
            }

            this.updateAppliedFilters = function (preset) {
                this.dropdown = constructObjectWithFilterNames(
                    mapFilterPresetTypesToMap.bind(this),
                    preset
                );

                var sortValues = this.appliedFilters.sortValues;

                this.appliedFilters = constructObjectWithFilterNames(
                    (dropdown, filter) =>
                        [...dropdown[filter.name]].map((f) => f[0]),
                    this.dropdown
                );

                this.appliedFilters.sortValues = sortValues;
            };
        }

        return {
            getEventReportFilter: function (pageType, filterTypes, columnCount) {
                return new eventReportFilter(pageType, filterTypes, columnCount);
            },
        };
    }
})();
