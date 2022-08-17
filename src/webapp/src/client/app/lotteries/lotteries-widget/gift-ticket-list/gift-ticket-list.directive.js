(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .directive('aceGiftTicketList', giftTicketList);

    function giftTicketList() {
        var directive = {
            templateUrl:
                'app/lotteries/lotteries-widget/gift-ticket-list/gift-ticket-list.html',
            restrict: 'E',
            replace: true,
            scope: {
                selectedUsers: '=',
                showCountError: '='
            },
            controller: giftTicketListController,
            controllerAs: 'vm',
            bindToController: true,
        };

        return directive;
    }

    giftTicketListController.$inject = ['$scope'];

    function giftTicketListController($scope) {
        var vm = this;

        vm.removeUser = removeUser;

        /*
        *   Because ng-repeat does not update the UI right away after removing the user from the list,
        *   the elements are hidden first with the use of CSS and deleted from the list after
        */
        function removeUser(user) {
            user.isDeleting = true;

            if (vm.selectedUsers.length == 1) {
                vm.disableHeader = true;
            }

            // Removing user after user is hidden
            setTimeout(function () {
                vm.selectedUsers = vm.selectedUsers.filter(
                    (selectedUser) => selectedUser.id !== user.id
                );

                vm.disableHeader = false;

                $scope.$apply();
            });
        }
    }
})();
