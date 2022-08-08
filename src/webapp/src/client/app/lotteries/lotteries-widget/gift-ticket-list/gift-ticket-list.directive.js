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
                selectedUsers: '='
            },
            controller: giftTicketListController,
            controllerAs: 'vm',
            bindToController: true,
        };

        return directive;
    }

    giftTicketListController.$inject = [
        'eventRepository',
        'notifySrv',
    ];

    function giftTicketListController(eventRepository, notifySrv) {
        var vm = this;
    }
})();
