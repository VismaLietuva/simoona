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
                // Disable scrolling while modal is open
                $(":root").css("overflow-y", 'hidden');

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
                })
                .closed
                .then(function() {
                    // Enable scrolling after modal is closed
                    $(":root").css("overflow-y", '');
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

        vm.minTicketCount = 1;
        vm.maxTicketCount = 10000;

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
        vm.giftedTicketsLimitExceeded = giftedTicketsLimitExceeded;
        vm.isTicketCountAnInteger = isTicketCountAnInteger;
        vm.onInvalidInputChangeToValidInput = onInvalidInputChangeToValidInput;

        vm.getUsers = getUsers;

        vm.selectingUsers = false;
        vm.selectedUsers = [];

        if ($window.lotteriesEnabled) {
            init();
        }

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
                getKudosAfterPurchase() >= 0 &&
                isTicketCountAnInteger()
            );
        }

        function canGiftTickets() {
            return (
                vm.selectedUsers.length > 0 &&
                getKudosAfterPurchase() >= 0 &&
                !giftedTicketsLimitExceeded()
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
            return vm.lottery.entryFee * getTotalTicketCount();
        }

        function getTotalTicketCount() {
            if (!vm.selectingUsers) {
                return vm.ticketCount;
            }

            return vm.selectedUsers.reduce(
                (accumulator, curr) => accumulator + curr.ticketCount,
                0
            );
        }

        function getKudosAfterPurchase() {
            return vm.lottery.buyer.remainingKudos - getTotalCost();
        }

        function getRemainingGiftedTicketCount() {
            return vm.lottery.buyer.remainingGiftedTicketCount - getTotalTicketCount();
        }

        function giftedTicketsLimitExceeded() {
            return getRemainingGiftedTicketCount() < 0;
        }

        function isTicketCountAnInteger() {
            return Number.isInteger(vm.ticketCount);
        }

        function onInvalidInputChangeToValidInput() {
            if (vm.ticketCount < vm.minTicketCount) {
                vm.ticketCount = vm.minTicketCount;
            } else if (!Number.isInteger(vm.ticketCount)) {
                vm.ticketCount = Math.round(vm.ticketCount);
            } else if (vm.ticketCount > vm.maxTicketCount) {
                vm.ticketCount = vm.maxTicketCount ;
            }
        }
    }
})();
