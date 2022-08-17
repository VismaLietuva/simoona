(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .directive('aceFlatTicketPicker', flatTickerPicker);

    function flatTickerPicker() {
        var directive = {
            templateUrl:
                'app/lotteries/lotteries-widget/gift-ticket-list/flat-ticket-picker/flat-ticket-picker.html',
            restrict: 'E',
            replace: true,
            scope: {
                ticketCount: '=',
                minTicketCount: '@',
                maxTicketCount: '@',
                showError: '='
            },
            controller: flatTickerPickerController,
            controllerAs: 'vm',
            bindToController: true,
        };

        return directive;
    }

    function flatTickerPickerController() {
        var vm = this;

        vm.increaseCount = increaseCount;
        vm.decreaseCount = decreaseCount;
        vm.onInvalidInputChangeToValidInput = onInvalidInputChangeToValidInput;

        vm.minimumCount = parseInt(vm.minTicketCount);
        vm.maximumCount = parseInt(vm.maxTicketCount);

        vm.ticketCount = vm.ticketCount || vm.minimumCount;

        function onInvalidInputChangeToValidInput() {
            if (vm.ticketCount < vm.minimumCount) {
                vm.ticketCount = vm.minimumCount;
            } else if (vm.ticketCount > vm.maximumCount) {
                vm.ticketCount = vm.maximumCount;
            } else if (!Number.isInteger(vm.ticketCount)) {
                vm.ticketCount = Math.round(vm.ticketCount);
            }
        }

        function increaseCount() {
            vm.ticketCount += 1;
        }

        function decreaseCount() {
            if (vm.ticketCount > vm.minimumCount) {
                vm.ticketCount -= 1;
            }
        }
    }
})();
