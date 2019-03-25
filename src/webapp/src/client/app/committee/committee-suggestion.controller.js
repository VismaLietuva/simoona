(function () {
    'use strict';

    angular.module('simoonaApp.Committee')
        .controller('CommitteeSuggestionModalController', CommitteeSuggestionModalController);

    CommitteeSuggestionModalController.$inject = ['$scope', '$uibModalInstance', 'committeeRepository', 'notifySrv', '$uibModal'];

    function CommitteeSuggestionModalController($scope, $uibModalInstance, committeeRepository, notifySrv, $uibModal) {

        $scope.isSaving = false;
        $scope.suggestion = {};

        $scope.submitSuggestion = function (suggestion) {
            $scope.isSaving = true;
            $scope.suggestion.committeeId = $scope.$parent.focusedCommittee.id;
            committeeRepository.postSuggestion(suggestion)
            .then(onSuccess, onError)
            .finally(function(){
                $scope.isSaving = false;
            })
        }

        $scope.cancel = function () {
            $uibModalInstance.close();
            $scope.getCommitteesList($scope.filter);
        };

        function onSuccess() {
            notifySrv.success('common.infoSaved');
            $uibModalInstance.close();
            $scope.getCommitteesList($scope.filter);
        };

        function onError(response) {
            notifySrv.error(response.data);
        };

        $scope.triggerComiteeChange = function (committee) {
            if ($scope.$parent.kudosCommitteeId !== 0 && committee.id !== $scope.$parent.kudosCommitteeId) {
                var deleteModal = $uibModal.open({
                    templateUrl: 'app/common/confirmation-popup/confirmation-popup.html',
                    controller: 'confirmationPopupController',
                    resolve: {
                        title: function () { return 'committee.kudosCommittee'; },
                        message: function () { return 'committee.kudosCommitteeAlreadyExsist'; },
                        callback: function () {
                            return function (ok) {
                                if (!ok) {
                                    committee.isKudosCommittee = !committee.isKudosCommittee;
                                }
                            }
                        },
                    }
                });
            }

        };

    }
})();
