(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceConfirmationModal', aceConfirmationModal);

    aceConfirmationModal.$inject = [
        '$uibModal'
    ];

    function aceConfirmationModal($uibModal) {
        var directive = {
            priority: 1,
            restrict: 'A',
            scope: {
                aceConfirmationModal: '&',
                confirmationModalConfirm: '@',
                confirmationModalReject: '@',
                confirmationModalMessage: '@',
                confirmationModalTitle: '@'
            },
            link: function (scope, elem) {
                elem.bind('click', function() {
                    $uibModal.open({
                        templateUrl: 'app/common/directives/confirmation-modal/confirmation-modal.html',
                        controller: confirmationModalController,
                        controllerAs: 'vm',
                        resolve: {
                            clickAction: function() {
                                return scope.aceConfirmationModal;
                            },
                            confirmationModalReject: function() {
                                return scope.confirmationModalReject;
                            },
                            confirmationModalConfirm: function() {
                                return scope.confirmationModalConfirm;
                            },
                            confirmationModalMessage: function() {
                                return scope.confirmationModalMessage;
                            },
                            confirmationModalTitle: function() {
                                return scope.confirmationModalTitle;
                            }
                        }
                    });
                });
            }
        };

        return directive;

    }

    confirmationModalController.$inject = [
        '$uibModalInstance',
        'clickAction',
        'confirmationModalReject',
        'confirmationModalConfirm',
        'confirmationModalMessage',
        'confirmationModalTitle'
    ];

    function confirmationModalController($uibModalInstance, clickAction, confirmationModalReject,
        confirmationModalConfirm, confirmationModalMessage, confirmationModalTitle) {
        /* jshint validthis: true */
        var vm = this;

        vm.confirmationModalTitle = confirmationModalTitle ? confirmationModalTitle : 'common.messageConfirm';
        vm.confirmationModalReject = confirmationModalReject ? confirmationModalReject : 'common.cancel';
        vm.confirmationModalConfirm = confirmationModalConfirm ? confirmationModalConfirm : 'common.delete';
        vm.confirmationModalMessage = confirmationModalMessage;

        vm.deleteItem = deleteItem;
        vm.closeModal = closeModal;

        ////////////

        function deleteItem() {
            clickAction();
            $uibModalInstance.close();
        }

        function closeModal() {
            $uibModalInstance.close();
        }

    }
})();
