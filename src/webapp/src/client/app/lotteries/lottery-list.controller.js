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
        .controller('lotteryListController', lotteryListController);

    lotteryListController.$inject = [
        '$rootScope',
        '$scope',
        'authService',
        '$location',
        'lotteryFactory',
        'lotteryStatuses'
    ];    

    function lotteryListController($rootScope, $scope, authService, $location, lotteryFactory, lotteryStatuses) {
    	/* jshint validthis: true */
        var vm = this;
        vm.lotteryStatuses = lotteryStatuses;
        $rootScope.pageTitle = 'lotteries.lotteriesPanelHeader';
        $scope.allowEdit = authService.hasPermissions(['ROLES_ADMINISTRATION']);

        init();

        function init() {
            lotteryFactory.getAllLotteries().then(function (response) {
                $scope.lotteries = response;
            })
        }
    }

})();
