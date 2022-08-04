(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .constant('lotteryImageSettings', {
            height: 165,
            width: 291,
        })
        .directive('aceLotteriesDetailModal', lotteryDetailModal);

    lotteryDetailModal.$inject = ['$uibModal'];

    function lotteryDetailModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceLotteriesDetailModal: '=?',
                onInit: '&',
            },
            link: linkFunc,
        };

        return directive;

        function linkFunc(scope, elem) {
            elem.bind('click', openLotteryWidget);

            onInitNotifyConsumer();

            function openLotteryWidget() {
                $uibModal.open({
                    templateUrl:
                        'app/lotteries/lotteries-widget/lotteries-detail-modal.html',
                    controller: lotteriesDetailController,
                    controllerAs: 'vm',
                    resolve: {
                        currentLottery: function () {
                            return scope.aceLotteriesDetailModal;
                        },
                    },
                });
            }

            function onInitNotifyConsumer() {
                scope.onInit({
                    lotteryId: scope.aceLotteriesDetailModal,
                    openLotteryWidget: openLotteryWidget,
                });
            }
        }
    }

    lotteriesDetailController.$inject = [
        '$uibModalInstance',
        'lotteryRepository',
        'currentLottery',
        'lotteryImageSettings',
        'notifySrv',
        'localeSrv',
        'errorHandler',
        '$window',
        'profileRepository',
    ];

    // TODO: add remaining kudos count ?
    function lotteriesDetailController(
        $uibModalInstance,
        lotteryRepository,
        currentLottery,
        lotteryImageSettings,
        notifySrv,
        localeSrv,
        errorHandler,
        $window,
        profileRepository
    ) {
        var vm = this;

        vm.lotteryImageSize = {
            w: lotteryImageSettings.width,
            h: lotteryImageSettings.height,
        };

        vm.ticketCount = 1;
        vm.currentLottery = currentLottery;

        vm.lotteryLoaded = false;
        vm.localeSrv = localeSrv;
        vm.notifySrv = notifySrv;
        vm.cancel = cancel;
        vm.ticketUp = ticketUp;
        vm.ticketDown = ticketDown;
        vm.buyTickets = buyTickets;
        vm.giftTickets = giftTickets;
        vm.canBuyTickets = canBuyTickets;
        vm.toggleUserSelection = toggleUserSelection;
        vm.cancelGiftingProcess = cancelGiftingProcess;
        vm.getTotalCost = getTotalCost;

        vm.getUsers = getUsers;
        vm.selectingUsers = false;
        vm.selectedUsers = [];

        if ($window.lotteriesEnabled) {
            init();
        }

        function init() {
            lotteryRepository
                .getLottery(currentLottery, {
                    includeRemainingKudos: true,
                })
                .then(function (lottery) {
                    vm.lottery = lottery;
                    vm.lotteryLoaded = true;
                });
        }

        function cancel() {
            $uibModalInstance.dismiss('cancel');
        }

        function ticketUp() {
            vm.ticketCount += 1;
        }

        function ticketDown() {
            if (vm.ticketCount > 1) {
                vm.ticketCount -= 1;
            }
        }

        function buyTickets() {
            if (vm.ticketCount > 0) {
                var lotteryTickets = {
                    lotteryId: currentLottery,
                    tickets: vm.ticketCount,
                    receivingUserIds:
                        vm.selectedUsers.length !== 0
                            ? vm.selectedUsers.map((user) => user.id)
                            : undefined,
                };

                lotteryRepository.buyTickets(lotteryTickets).then(
                    function () {
                        vm.notifySrv.success(
                            vm.localeSrv.formatTranslation(
                                'lotteries.hasBeenBought',
                                {
                                    one:
                                        vm.ticketCount *
                                        getTicketReceiversCount(),
                                    two: vm.lottery.title,
                                }
                            )
                        );
                        $uibModalInstance.close();
                    },
                    function (error) {
                        errorHandler.handleErrorMessage(error);
                    }
                );
            } else {
                vm.notifySrv.error(
                    vm.localeSrv.formatTranslation(
                        'lotteries.invalidTicketNumber'
                    )
                );
            }
        }

        function giftTickets() {
            buyTickets();
        }

        function canBuyTickets() {
            return vm.ticketCount > 0 && !vm.selectingUsers;
        }

        function toggleUserSelection() {
            vm.selectingUsers = !vm.selectingUsers;
        }

        function cancelGiftingProcess() {
            vm.selectedUsers = [];
            toggleUserSelection();
        }

        function getTicketReceiversCount() {
            return vm.selectedUsers.length || 1;
        }

        function getUsers(search) {
            return profileRepository.getUserForAutoComplete({
                s: search,
            });
        }

        function getTotalCost() {
            return (
                vm.lottery.entryFee * vm.ticketCount * getTicketReceiversCount()
            );
        }
    }
})();
