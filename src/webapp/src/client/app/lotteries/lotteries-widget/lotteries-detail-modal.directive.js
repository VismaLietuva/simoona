(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .constant('lotteryImageSettings', {
            height: 165,
            width: 291,
        })
        .directive('aceLotteriesDetailModal', kudosifyModal)

    kudosifyModal.$inject = [
        '$uibModal'
    ];

    function kudosifyModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceLotteriesDetailModal: '=?'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, elem) {
            elem.bind('click', function () {
                $uibModal.open({
                    templateUrl: 'app/lotteries/lotteries-widget/lotteries-detail-modal.html',
                    controller: lotteriesDetailController,
                    controllerAs: 'vm',
                    resolve: {
                        currentLottery: function () {
                            return scope.aceLotteriesDetailModal;
                        }
                    }
                });
            });
        }
    }

    lotteriesDetailController.$inject = [
        '$scope',
        '$uibModalInstance',
        'authService',
        'lodash',
        'dataHandler',
        'errorHandler',
        'lotteryFactory',
        'currentLottery',
        'lotteryImageSettings'
    ];

    function lotteriesDetailController($scope, $uibModalInstance, authService, 
        lodash, dataHandler, errorHandler, lotteryFactory, currentLottery, lotteryImageSettings) {
        var vm = this;
        vm.lotteryImageSize = {
            w: lotteryImageSettings.width,
            h: lotteryImageSettings.height
        };
        vm.cancel = cancel;
        vm.currentLottery = currentLottery;
        init();
        
        //////

        function init() {

            lotteryFactory.getLottery(currentLottery)
            .then(function(lottery){
                vm.lottery = lottery;
                console.log(vm.lottery);
            });
        }
        function cancel() {
            $uibModalInstance.dismiss('cancel');
        }
    }
})();
