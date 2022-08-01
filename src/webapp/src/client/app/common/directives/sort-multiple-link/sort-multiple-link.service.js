(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('sortMultipleLinkService', sortMultipleLinkService);

    function sortMultipleLinkService() {
        function multipleSort(columnCount) {
            this.sortValues = {
                sortBy: new Array(columnCount).fill(undefined),
                sortOrders: new Array(columnCount).fill(undefined),
                sortPriorities: []
            };

            this.setSortValues = function (sortBy, sortOrder, position) {
                this.sortValues.sortBy[position] = sortBy;
                this.sortValues.sortOrders[position] = sortOrder;

                if (sortBy !== undefined &&
                    sortOrder !== undefined &&
                    this.sortValues.sortPriorities.find(priority => priority === position) == null) {
                    this.sortValues.sortPriorities.push(position);
                } else if (sortBy === undefined || sortOrder === undefined) {
                    this.sortValues.sortPriorities = this.sortValues.sortPriorities
                        .filter(priority => priority !== position);
                }
            }

            this.getSortString = function () {
                var sortString = "";

                for (var priority of this.sortValues.sortPriorities) {
                    var sortBy = this.sortValues.sortBy[priority];
                    var sortOrder = this.sortValues.sortOrders[priority];

                    sortString += `${sortBy} ${sortOrder};`;
                }

                if (sortString == "") {
                    return undefined;
                }

                return sortString;
            }
        }

        return {
            getMultipleSort: function (columnCount) {
                return new multipleSort(columnCount);
            },
        };
    }
})();
