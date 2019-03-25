(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .directive('aceKudosRejectionModal', kudosRejectionModal);

    kudosRejectionModal.$inject = [
        '$uibModal'
    ];

    function kudosRejectionModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceKudosRejectionModal: '=',
                onReject: '&'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, elem) {
            elem.bind('click', function () {
                $uibModal.open({
                    templateUrl: 'app/kudos/rejection-modal/rejection-modal.html',
                    controller: kudosRejectionModalController,
                    controllerAs: 'vm',
                    resolve: {
                        kudos: function () {
                            return scope.aceKudosRejectionModal;
                        },
                        onRejectAction: function () {
                            return scope.onReject;
                        }
                    }
                });
            });
        }
    }

    kudosRejectionModalController.$inject = [
        '$uibModalInstance',
        'kudos',
        'onRejectAction',
        'kudosFactory',
        'notifySrv'
    ];

    function kudosRejectionModalController($uibModalInstance, kudos, onRejectAction,
       kudosFactory, notifySrv) {
        /*jshint validthis: true */
        var vm = this;

        vm.rejectForm = rejectForm;
        vm.cancelForm = cancelForm;

        vm.kudosRejectionMessage = '';

        //////

        function rejectForm() {
            kudosFactory.rejectKudos(kudos.id, vm.kudosRejectionMessage).then(function () {
                onRejectAction();

                notifySrv.success('kudos.kudosRejectionSuccessfullySaved');
                $uibModalInstance.close();
            }, function () {
                if (vm.kudosRejectionMessage.length > 300) {
                    notifySrv.error('kudos.kudosRejectionMessageIsTooLong');
                } else if (vm.kudosRejectionMessage.length === 0) {
                    notifySrv.error('kudos.kudosRejectionMessageIsEmpty');
                } else {
                    notifySrv.error('kudos.kudosRejectionInvalidKudos');
                }
            });
        }

        function cancelForm() {
            $uibModalInstance.dismiss('cancel');
        }
    }
})();
