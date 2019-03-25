(function() {
    'use strict';

    angular
        .module('simoonaApp.Widget.KudosBasket')
        .directive('aceKudosBasketWidget', kudosBasketWidget);

    function kudosBasketWidget() {
        var directive = {
            restrict: 'E',
            scope: {},
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

        var hasKudosBasketPermission = authService.hasPermissions(['KUDOSBASKET_BASIC']) || authService.hasPermissions(['KUDOSBASKET_ADMINISTRATION']);

        vm.kudosBasketData = {};
        vm.kudosBasketIsActive = false;

        vm.addDonation = addDonation;

        init();

        ////////////

        function init() {
            if (hasKudosBasketPermission) {
                kudosBasketRepository.getKudosBasketWidget().then(function(response) {
                    if (JSON.stringify(response) !== '{}') {
                        vm.kudosBasketIsActive = true;
                    }

                    vm.kudosBasketData = response;
                });
            }
        }

        function addDonation(donatedAmount) {
            vm.kudosBasketData.kudosDonated = parseFloat(vm.kudosBasketData.kudosDonated) + parseFloat(donatedAmount);
            vm.kudosBasketData.kudosDonated = (vm.kudosBasketData.kudosDonated.toFixed(2) * 1).toString();
        }

    }
})();
