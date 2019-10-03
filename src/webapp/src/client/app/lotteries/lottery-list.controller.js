(function() {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .controller('lotteryListController', lotteryListController);

    lotteryListController.$inject = [
        '$rootScope',
        '$scope',
        'authService',
        '$location'
    ];    

    function lotteryListController($rootScope, $scope, authService, $location) {
    	/* jshint validthis: true */
        var vm = this;
        $rootScope.pageTitle = 'lotteries.lotteriesPanelHeader';
        $scope.allowEdit = authService.hasPermissions(['ROLES_ADMINISTRATION']);
        $scope.lotteries = {pagedList: [
            {
                name: "Paspirtukas",
                status: "started",
                endDate: Date.now()
            },
            {
                name: "100 Kudosu",
                status: "ended",
                endDate: Date.now()
            }
        ]}
    }

})();
