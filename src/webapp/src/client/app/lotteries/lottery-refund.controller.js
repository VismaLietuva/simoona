(function() {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .controller('lotteryRefundController', lotteryRefundController);

        lotteryRefundController.$inject = [
        '$rootScope',
        '$scope',
        '$state',
        'lotteryRepository',
        'errorHandler',
        'notifySrv',
        'lotteryStatuses'
    ];    

    function lotteryRefundController($rootScope, $scope, $state, lotteryRepository, errorHandler, notifySrv, lotteryStatuses) {
    	/* jshint validthis: true */
        var vm = this;
        $rootScope.pageTitle = 'lotteries.lotteriesPanelHeader';
        
        vm.lotteryId = $rootScope.$stateParams.lotteryId;
        vm.refundLottery = refundLottery;

        init();

        function init() {
            lotteryRepository.getLotteryStatus(vm.lotteryId).then(function (response) {
                vm.refundFailed = response.refundFailed;
                vm.lotteryStatus = response.lotteryStatus;
            });
        }

        function refundLottery() {
            lotteryRepository.refundParticipants(vm.lotteryId).
                then(function() {
                    notifySrv.success('lotteries.startedRefunding');
                    $state.go('Root.WithOrg.Admin.Lotteries.List');
                }, errorHandler.handleErrorMessage);
        }
    }

})();
