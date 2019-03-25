(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('itemFilterPanelFactory', itemFilterPanelFactory);

    itemFilterPanelFactory.$inject = ['lodash'];

    function itemFilterPanelFactory(lodash) {
        var service = {
            getFilterParameters: getFilterParameters,
            executeFilter: executeFilter
        };

        return service;

        ////////////

        function getFilterParameters(filters, filterJson) {
            var values = '';
            var filterObj = {
                filters: filters,
                isOpen: false
            };
            var filterParams = null;
            if (!!filterJson) {
                filterParams = JSON.parse(filterJson);
            }
            filterObj.filters.forEach(function(filterItem) {
                values = lodash(filterParams).filter(function(item) { return item.key === filterItem.filter; }).map('values').first();

                if (!!values) {
                    filterItem.model = values;
                    filterObj.isOpen = true;
                }

            });
            return filterObj;
        }

        function executeFilter(filters) {
            var filterParams = [];
            filters.forEach(function(filterItem) {
                if (filterItem.model && filterItem.model.length > 0) {
                    var filterValues = [];

                    filterItem.model.forEach(function(modelItem) {
                        filterValues.push(modelItem.name);
                    });

                    filterParams.push({
                        key: filterItem.filter,
                        values: filterValues
                    });
                }
            });

            return filterParams;
        }
    }
})();
