(function() {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .constant('lotteryStatuses', {
            1: "Drafted",
            2: "Started",
            3: "Aborted",
            4: "Ended"
        })
        .constant('lotteryPageSettings', {
            'pageSize': 10
        })
        .controller('lotteryListController', lotteryListController);

    lotteryListController.$inject = [
        '$rootScope',
        '$scope',
        'authService',
        '$location',
        'lotteryFactory',
        'lotteryStatuses',
        'lotteryPageSettings',
    ];    

    function lotteryListController($rootScope, $scope, authService, $location, lotteryFactory, lotteryStatuses, lotteryPageSettings) {
    	/* jshint validthis: true */
        var vm = this;
        vm.lotteryStatuses = lotteryStatuses;
        vm.onSearch = onSearch;
        vm.filters = lotteryPageSettings;
        vm.onPageChange = onPageChange;
        $rootScope.pageTitle = 'lotteries.lotteriesPanelHeader';
        vm.allowEdit = authService.hasPermissions(['ROLES_ADMINISTRATION']);

        init();

        function init() {
            lotteryFactory.getLotteryListPaged(vm.filters).then(function (response) {
                vm.lotteries = response.pagedList;
                vm.filters.itemCount = response.itemCount;
            })
        }

        function onSearch(searchString) {
            vm.filters.s = searchString;
            vm.filters.page = 1;
            changeState();
        }

        function onPageChange() {
            changeState();
        }

        function changeState() {
            var filterParams = {};
            if (!!vm.filters.page) {
                filterParams.page = vm.filters.page;
            }
            if (!!vm.filters.s) {
                filterParams.filter = vm.filters.s;
            }
            lotteryFactory.getLotteryListPaged(filterParams).then(function (lotteries) {
                vm.lotteries = lotteries.pagedList;
                vm.filters.itemCount = lotteries.itemCount;
            });
        }
    }

})();
