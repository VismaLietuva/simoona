(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .factory('eventReportService', eventReportService);

    function eventReportService() {
        function eventReportFilter(pageType, ...filterTypes) {
            function constructObjectWithFilterNames(func, response) {
                var property = {};

                filterTypes.forEach((filter) => {
                    property = {
                        ...property,
                        [filter.name]: func(response, filter),
                    };
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

            this.appliedFilters = {
                ...this.appliedFilters,
                sortBy: undefined,
                sortOrder: undefined,
            };

            this.setFilterTypes = function (response) {
                this.filterTypes = constructObjectWithFilterNames(
                    findFilterByType,
                    response
                );
            };

            this.setSortValues = function (sortBy, sortOrder) {
                this.appliedFilters.sortBy = sortBy;
                this.appliedFilters.sortOrder = sortOrder;
            }

            this.updateAppliedFilter = function (filter, filterName) {
                this.dropdown[filterName] = new Map(filter);
                this.appliedFilters[filterName] = [...this.dropdown[filterName]]
                    .map(f => f[0]);
            }

            this.updateAppliedFilters = function (preset) {
                this.dropdown = constructObjectWithFilterNames(
                    mapFilterPresetTypesToMap.bind(this),
                    preset
                );

                this.appliedFilters = constructObjectWithFilterNames(
                    (dropdown, filter) =>
                        [...dropdown[filter.name]].map((f) => f[0]),
                    this.dropdown
                );
            };
        }

        return {
            getEventReportFilter: function (pageType, ...filterTypes) {
                return new eventReportFilter(pageType, filterTypes);
            },
        };
    }
})();
