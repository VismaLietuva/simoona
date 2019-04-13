(function() {
    'use strict';

    angular
        .module('simoonaApp.Widget.KudosBasket')
        .directive('aceKudosBasketWidget', kudosBasketWidget);

    function kudosBasketWidget() {
        var directive = {
            restrict: 'E',
            scope: {
                kudosBasketData: '=?'
            },
            templateUrl: 'app/widget/kudos-basket/widget/widget.html',
            bindToController: true,
            controller: kudosBasketWidgetController,
            controllerAs: 'vm'
        };
        return directive;
    }

    kudosBasketWidgetController.$inject = [
        'kudosBasketRepository',
        'authService'
    ];

    function kudosBasketWidgetController(kudosBasketRepository, authService) {
        /* jshint validthis: true */
        var vm = this;

        var hasPermissions = hasPermissions();
        vm.kudosBasketIsActive = function () { 
            return vm.kudosBasketData && hasPermissions;
        };

        vm.addDonation = addDonation;

        ////////////

        function addDonation(donatedAmount) {
            vm.kudosBasketData.kudosDonated = parseFloat(vm.kudosBasketData.kudosDonated) + parseFloat(donatedAmount);
            vm.kudosBasketData.kudosDonated = (vm.kudosBasketData.kudosDonated.toFixed(2) * 1).toString();
        }

        function hasPermissions() {
            return authService.hasPermissions(['KUDOSBASKET_BASIC']) || authService.hasPermissions(['KUDOSBASKET_ADMINISTRATION']);
        }
    }
})();
