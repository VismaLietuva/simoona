(function() {
    'use strict';

    angular
        .module('simoonaApp.Widget.KudosBasket')
        .directive('aceKudosBasketModal', kudosBasketModal);

    kudosBasketModal.$inject = ['$uibModal'];

    function kudosBasketModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceKudosBasketModal: '&'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, elem) {
            elem.bind('click', function() {
                $uibModal.open({
                    templateUrl: 'app/widget/kudos-basket/modal/modal.html',
                    controller: kudosBasketModalController,
                    controllerAs: 'vm',
                    windowClass: 'kudos-basket-modal',
                    resolve: {
                        addDonation: function() {
                            return scope.aceKudosBasketModal();
                        }
                    }
                });
            });
        }
    }

    kudosBasketModalController.$inject = [
        '$rootScope',
        '$scope',
        '$uibModalInstance',
        'notifySrv',
        'kudosBasketRepository',
        'addDonation'
    ];

    function kudosBasketModalController($rootScope, $scope, $uibModalInstance,
        notifySrv, kudosBasketRepository, addDonation) {
        /* jshint validthis: true */
        var vm = this;

        vm.isBusy = false;

        vm.submitDonation = submitDonation;
        vm.onChangeFixValidity = onChangeFixValidity;
        vm.closeModal = closeModal;

        init();
        ////////////

        function init() {
            kudosBasketRepository.getKudosBasketWidget().then(function(response) {
                    if (response && response.Message) {
                        notifySrv.error(response.Message);
                        closeModal();
                    } else {
                        vm.kudosBasketData = response;
                    }
                });
        }

        function onChangeFixValidity() {
            if (vm.amount) {
                vm.amount = vm.amount.replace(',', '.');
            }

            minValidator(vm.amount);
        }

        function submitDonation() {
            vm.isBusy = true;
            kudosBasketRepository.makeDonation(vm.kudosBasketData, vm.amount).then(function() {
                    notifySrv.success('kudosBasket.donatedSuccessfully');
                    addDonation(vm.amount);
                    $uibModalInstance.close();

                    $rootScope.$broadcast('addKudosEvent');
                },
                function(response) {
                    notifySrv.error(response.data.message);
                    vm.isBusy = false;
                });
        }

        function closeModal() {
            $uibModalInstance.close();
        }

        function minValidator(value) {
            var min = angular.element('#amount').attr('ng-min');
            if (value < min) {
                $scope.kudosBasketForm.amount.$setValidity('ngMin', false);
                return undefined;
            } else {
                $scope.kudosBasketForm.amount.$setValidity('ngMin', true);
                return value;
            }
        }

    }
})();
