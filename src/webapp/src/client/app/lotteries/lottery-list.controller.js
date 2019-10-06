(function() {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .controller('lotteryListController', lotteryListController);

    lotteryListController.$inject = [
        '$rootScope',
        '$scope',
        'authService',
        '$location',
        'lotteryFactory'
    ];    

    function lotteryListController($rootScope, $scope, authService, $location, lotteryFactory) {
    	/* jshint validthis: true */
        var vm = this;
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
