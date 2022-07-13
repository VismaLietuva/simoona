(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .factory('eventReportService', eventReportService);

    function eventReportService() {
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

            function copyObjectWithEmptySortValues(object) {
                return Object.assign({
                    sortBy: new Array(columnCount).fill(undefined),
                    sortOrders: new Array(columnCount).fill(undefined),
                    sortPriorities: []
                }, object);
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

            this.appliedFilters = copyObjectWithEmptySortValues(this.appliedFilters);

            this.setFilterTypes = function (response) {
                this.filterTypes = constructObjectWithFilterNames(
                    findFilterByType,
                    response
                );
            };

            this.setSortValues = function (sortBy, sortOrder, position) {
                this.appliedFilters.sortBy[position] = sortBy;
                this.appliedFilters.sortOrders[position] = sortOrder;

                if (sortBy !== undefined &&
                    sortOrder !== undefined &&
                    this.appliedFilters.sortPriorities.find(priority => priority === position) == null) {
                    this.appliedFilters.sortPriorities.push(position);
                } else if (sortBy === undefined || sortOrder === undefined) {
                    this.appliedFilters.sortPriorities = this.appliedFilters.sortPriorities
                        .filter(priority => priority !== position);
                }
            }

            this.updateAppliedFilter = function (filter, filterName) {
                this.dropdown[filterName] = new Map(filter);
                this.appliedFilters[filterName] = [...this.dropdown[filterName]]
                    .map(f => f[0]);
            }

            this.getSortString = function () {
                var sortString = "";

                for (var priority of this.appliedFilters.sortPriorities) {
                    var sortBy = this.appliedFilters.sortBy[priority];
                    var sortOrder = this.appliedFilters.sortOrders[priority];

                    sortString += `${sortBy} ${sortOrder};`;
                }

                return sortString;
            }

            this.updateAppliedFilters = function (preset) {
                this.dropdown = constructObjectWithFilterNames(
                    mapFilterPresetTypesToMap.bind(this),
                    preset
                );

                var sortBy = this.appliedFilters.sortBy;
                var sortOrders = this.appliedFilters.sortOrders;
                var sortPriorities = this.appliedFilters.sortPriorities;

                this.appliedFilters = constructObjectWithFilterNames(
                    (dropdown, filter) =>
                        [...dropdown[filter.name]].map((f) => f[0]),
                    this.dropdown
                );

                this.appliedFilters.sortBy = sortBy;
                this.appliedFilters.sortOrders = sortOrders;
                this.appliedFilters.sortPriorities = sortPriorities;
            };
        }

        return {
            getEventReportFilter: function (pageType, filterTypes, columnCount) {
                return new eventReportFilter(pageType, filterTypes, columnCount);
            },
        };
    }
})();
