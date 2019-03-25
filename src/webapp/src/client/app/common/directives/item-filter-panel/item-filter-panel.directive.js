(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceItemFilterPanel', itemFilterPanel);

    function itemFilterPanel() {
        var directive = {
            restrict: 'AE',
            replace: true,
            templateUrl: 'app/common/directives/item-filter-panel/item-filter-panel.html',
            scope: {
                isOpen: '=',
                filterTitle: '=',
                filterList: '=',
                filterParams: '=',
                triggerItemFiltering: '='
            },
            controller: itemFilterPanelController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;

    }

    itemFilterPanelController.$inject = ['$location', '$translate', 'itemFilterPanelFactory'];

    function itemFilterPanelController($location,$translate, itemFilterPanelFactory) {
        /* jshint validthis: true */
        var vm = this;
        vm.executeFilter = executeFilter;


        init();

        ////////////

        function init() {
            handleFilterParams();
        }

        function executeFilter() {
            if (vm.triggerItemFiltering) {
                vm.triggerItemFiltering(itemFilterPanelFactory.executeFilter(vm.filterList));
            }
        }

        function handleFilterParams() {
            var searches = $location.search();
            var filterJson = searches['filter'];
            var filterObj = itemFilterPanelFactory.getFilterParameters(vm.filterList, filterJson);
            vm.filterList = filterObj.filters;
            vm.isOpen = filterObj.isOpen;
        }


    }
})();
