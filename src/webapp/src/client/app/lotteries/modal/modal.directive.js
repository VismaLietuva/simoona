(function() {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .directive('aceLotteryStatsModal', lotteryStatsModal);

    lotteryStatsModal.$inject = ['$uibModal'];

    function lotteryStatsModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceLotteryStatsModal: '&'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, elem) {
            elem.bind('click', function() {
                $uibModal.open({
                    templateUrl: 'app/lotteries/modal/modal.html',
                    controller: lotteryStatsController,
                    controllerAs: 'vm',
                });
            });
        }
    }

    lotteryStatsController.$inject = [
        '$rootScope',
        '$scope',
        '$uibModalInstance',
        'notifySrv',
        'lotteryRepository'
    ];

    function lotteryStatsController($rootScope, $scope, $uibModalInstance,
        notifySrv, lotteryRepository) {
        /* jshint validthis: true */
        var vm = this;
        vm.lotteryId = $rootScope.$stateParams.lotteryId;
        vm.filters = {
            id: vm.lotteryId,
            page: 1
        }
        vm.closeModal = closeModal;
        vm.onPageChange = onPageChange;
        init();
        ////////////

        function init() {
            lotteryRepository.getLotteryStatistics(vm.lotteryId).then(function(response) {
                    if (response && response.Message) {
                        notifySrv.error(response.Message);
                        closeModal();
                    } else {
                        vm.lotteryData = response;
                    }
                });
        
            lotteryRepository.getLotteryParticipants(vm.filters).then(function(response) {
                if (response && response.Message) {
                    notifySrv.error(response.Message);
                    closeModal();
                } else {
                    vm.participants = response;
                }
            })
        }

        function closeModal() {
            $uibModalInstance.close();
        }

        function onPageChange () {
            lotteryRepository.getLotteryParticipants(vm.filters).then(function(response) {
                if (response && response.Message) {
                    notifySrv.error(response.Message);
                    closeModal();
                } else {
                    vm.participants = response;
                }
            });
        }
    }
})();
