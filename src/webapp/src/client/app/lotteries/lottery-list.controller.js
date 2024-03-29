(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .constant('editableLotteries', ['drafted', 'started'])
        .constant('lotteryPageSettings', {
            pageSize: 10,
        })
        .controller('lotteryListController', lotteryListController);

    lotteryListController.$inject = [
        '$rootScope',
        'authService',
        'lotteryRepository',
        'lotteryStatuses',
        'lotteryPageSettings',
        'editableLotteries',
    ];

    function lotteryListController(
        $rootScope,
        authService,
        lotteryRepository,
        lotteryStatuses,
        lotteryPageSettings,
        editableLotteries
    ) {
        /* jshint validthis: true */
        var vm = this;
        vm.lotteryStatuses = lotteryStatuses;
        vm.editableLotteries = editableLotteries;
        vm.onSearch = onSearch;
        vm.filters = lotteryPageSettings;
        vm.onPageChange = onPageChange;
        vm.getLotteryStatusString = getLotteryStatusString;
        vm.isLotteryEditable = isLotteryEditable;
        $rootScope.pageTitle = 'lotteries.lotteriesPanelHeader';
        vm.allowEdit = authService.hasPermissions(['LOTTERY_ADMINISTRATION']);
        vm.showRefundPage = showRefundPage;
        vm.onSort = onSort;

        init();

        function init() {
            lotteryRepository
                .getLotteryListPaged(vm.filters)
                .then(function (response) {
                    vm.lotteries = response.pagedList;
                    vm.filters.itemCount = response.itemCount;
                });
        }

        function onSearch(searchString) {
            onCompleteLoadFirstPage(function() {
                vm.filters.searchString = searchString;
            });
        }

        function onPageChange() {
            changeState();
        }

        function changeState() {
            var filterParams = { sortByProperties: vm.filters.sortString };

            if (!!vm.filters.page) {
                filterParams.page = vm.filters.page;
            }

            if (!!vm.filters.searchString) {
                filterParams.filter = vm.filters.searchString;
            }

            lotteryRepository
                .getLotteryListPaged(filterParams)
                .then(function (lotteries) {
                    vm.lotteries = lotteries.pagedList;
                    vm.filters.itemCount = lotteries.itemCount;
                });
        }

        function getLotteryStatusString(status) {
            return (
                'lotteries.' +
                Object.keys(vm.lotteryStatuses).find(
                    (key) => vm.lotteryStatuses[key] === status
                )
            );
        }

        function isLotteryEditable(lottery) {
            return vm.editableLotteries.some(
                (status) => vm.lotteryStatuses[status] === lottery.status
            );
        }

        function showRefundPage(lottery) {
            return lottery.refundFailed;
        }

        function onSort(sortValue, sortDirection) {
            onCompleteLoadFirstPage(function() {
                vm.filters.sortString = `${sortValue} ${sortDirection}`;
            });
        }

        function onCompleteLoadFirstPage(func) {
            func();

            vm.filters.page = 1;
            changeState();
        }
    }
})();
