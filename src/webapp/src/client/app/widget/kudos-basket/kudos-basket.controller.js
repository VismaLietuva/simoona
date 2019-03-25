(function() {
    'use strict';

    angular
        .module('simoonaApp.Widget.KudosBasket')
        .controller('KudosBasketController', kudosBasketController);

    kudosBasketController.$inject = ['$state', 'appConfig', 'kudosBasketRepository', 'notifySrv'];

    function kudosBasketController($state, appConfig, kudosBasketRepository, notifySrv) {
        /*jshint validthis: true */
        var vm = this;

        vm.saveKudosBasket = saveKudosBasket;
        vm.cancelKudosBasket = cancelKudosBasket;
        vm.deleteKudosBasket = deleteKudosBasket;

        init();

        //////////////

        function init() {
            vm.kudosBasketData = {};
            vm.donatorsUsersList = [];

            kudosBasketRepository.getKudosBasket().then(function(response) {
                vm.kudosBasketData = response;
                if (!!vm.kudosBasketData.id) {
                    getDonations();
                }
            });

        }

        function saveKudosBasket() {
            if (vm.kudosBasketData.id) {
                kudosBasketRepository.editKudosBasket(vm.kudosBasketData).then(function(response) {
                    vm.kudosBasketData = response;

                    notifySrv.success('kudosBasket.updatedKudosBasketSuccessfully');
                });
            } else {
                kudosBasketRepository.createNewBasket(vm.kudosBasketData).then(function(response) {
                    vm.kudosBasketData = response;

                    notifySrv.success('kudosBasket.createdKudosBasketSuccessfully');
                });
            }
        }

        function getDonations() {
            kudosBasketRepository.getDonations().then(function(response) {
                vm.donatorsUsersList = response;
            });
        }

        function cancelKudosBasket() {
            $state.go(appConfig.homeStateName, {}, {
                reload: true
            });
        }

        function deleteKudosBasket() {
            kudosBasketRepository.deleteKudosBasket().then(function() {
                notifySrv.success('kudosBasket.deletedKudosBasketSuccessfully');
                vm.kudosBasketData = {};
                vm.donatorsUsersList = [];
            },
            function(response) {
                notifySrv.error(response.data.message);
            });
        }
    }
})();
