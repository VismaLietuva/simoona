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
        'lotteryRepository',
        'Analytics'
    ];

    function lotteryStatsController($rootScope, $scope, $uibModalInstance,
        notifySrv, lotteryRepository ,Analytics) {
        /* jshint validthis: true */
        var vm = this;
        vm.lotteryId = $rootScope.$stateParams.lotteryId;

        vm.closeModal = closeModal;
        vm.exportParticipantsData = exportParticipantsData;
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
        }

        function closeModal() {
            $uibModalInstance.close();
        }
        function exportParticipantsData()
        {
            Analytics.trackEvent('Lotteries', 'Export participant list', 'lottery: ' + vm.lotteryId);
            lotteryRepository.exportParticipants(vm.lotteryId).then(function(response) {
                var file = new Blob([response.data], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;'
                });
                saveAs(file, 'participants.xlsx');
            });
        }

    }
})();
