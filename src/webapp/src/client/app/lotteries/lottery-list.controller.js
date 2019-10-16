(function() {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .constant('lotteryStatuses', {
            1: "drafted",
            2: "started",
            3: "aborted",
            4: "ended"
        })
        .constant('editableLotteries', [1, 2])
        .constant('lotteryPageSettings', {
            'pageSize': 10
        })
        .controller('lotteryListController', lotteryListController);

    lotteryListController.$inject = [
        '$rootScope',
        '$scope',
        'authService',
        '$location',
        'lotteryRepository',
        'lotteryStatuses',
        'lotteryPageSettings',
        'editableLotteries'
    ];    

    function lotteryListController($rootScope, $scope, authService, $location, lotteryRepository, lotteryStatuses, lotteryPageSettings, editableLotteries) {
    	/* jshint validthis: true */
        var vm = this;
        vm.lotteryStatuses = lotteryStatuses;
        vm.editableLotteries = editableLotteries;
        vm.onSearch = onSearch;
        vm.filters = lotteryPageSettings;
        vm.onPageChange = onPageChange;
        $rootScope.pageTitle = 'lotteries.lotteriesPanelHeader';
        vm.allowEdit = authService.hasPermissions(["LOTTERY_ADMINISTRATION"]);

        init();

        function init() {
            lotteryRepository.getLotteryListPaged(vm.filters).then(function (response) {
                vm.lotteries = response.pagedList;
                vm.filters.itemCount = response.itemCount;
            })
        }

        function onSearch(searchString) {
            vm.filters.searchString = searchString;
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
            if (!!vm.filters.searchString) {
                filterParams.filter = vm.filters.searchString;
            }
            lotteryRepository.getLotteryListPaged(filterParams).then(function (lotteries) {
                vm.lotteries = lotteries.pagedList;
                vm.filters.itemCount = lotteries.itemCount;
            });
        }
    }

})();
