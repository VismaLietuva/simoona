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
        vm.canGiftTickets = canGiftTickets;
        vm.toggleUserSelection = toggleUserSelection;
        vm.cancelGiftingProcess = cancelGiftingProcess;
        vm.getTotalCost = getTotalCost;
        vm.getKudosAfterPurchase = getKudosAfterPurchase;
        vm.getRemainingGiftedTicketCount = getRemainingGiftedTicketCount;

        vm.getUsers = getUsers;

        vm.selectingUsers = false;
        vm.selectedUsers = [];

        if ($window.lotteriesEnabled) {
            init();
        }
        // TODO: add validation message
        // TODO: fix ticket count validation
        function init() {
            lotteryRepository
                .getLottery(currentLottery, {
                    includeBuyer: true,
                })
                .then(function (lottery) {
                    vm.lottery = lottery;
                    vm.lottery.canGiftTickets =
                        vm.lottery.giftedTicketLimit > 0 &&
                        vm.lottery.buyer.remainingGiftedTicketCount > 0;

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
                var giftingTickets = vm.selectedUsers.length !== 0;

                var lotteryTickets = {
                    lotteryId: currentLottery,
                    tickets: vm.ticketCount,
                    receivingUserIds: giftingTickets
                        ? vm.selectedUsers.map((user) => user.id)
                        : undefined,
                };

                lotteryRepository.buyTickets(lotteryTickets).then(
                    function () {
                        var translation = giftingTickets
                            ? 'lotteries.hasBeenGifted'
                            : 'lotteries.hasBeenBought';

                        vm.notifySrv.success(
                            vm.localeSrv.formatTranslation(translation, {
                                one: vm.ticketCount * getTicketReceiversCount(),
                                two: vm.lottery.title,
                            })
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
            return (
                vm.ticketCount > 0 &&
                !vm.selectingUsers &&
                getKudosAfterPurchase() >= 0
            );
        }

        function canGiftTickets() {
            return (
                vm.selectedUsers.length > 0 &&
                getKudosAfterPurchase() >= 0 &&
                getRemainingGiftedTicketCount() >= 0
            );
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
            if (!vm.lottery) {
                return 0;
            }

            return (
                vm.lottery.entryFee * vm.ticketCount * getTicketReceiversCount()
            );
        }

        function getKudosAfterPurchase() {
            if (!vm.lottery) {
                return 0;
            }

            return vm.lottery.buyer.remainingKudos - getTotalCost();
        }

        function getRemainingGiftedTicketCount() {
            if (!vm.lottery) {
                return 0;
            }

            var remainingGiftedTicketCount =
                vm.lottery.buyer.remainingGiftedTicketCount -
                vm.ticketCount * getTicketReceiversCount();

            if (remainingGiftedTicketCount < 0) {
                return 0;
            }

            return remainingGiftedTicketCount;
        }
    }
})();
