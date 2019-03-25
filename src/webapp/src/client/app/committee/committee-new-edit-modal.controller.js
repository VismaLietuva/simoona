(function () {
    'use strict';

    angular
        .module('simoonaApp.Committee')
        .constant('committeeImageConfig', {
            thumbHeight: 200,
            thumbWidth: 200
        })
        .controller('newCommitteeModalController', newCommitteeModalController);

    newCommitteeModalController.$inject = ['$state', '$scope', '$uibModalInstance', 'committeeRepository', 'pictureRepository',
        'shroomsFileUploader', 'notifySrv', '$uibModal', 'imageValidationSettings', 'committeeImageConfig', 'authService'];

    function newCommitteeModalController($state, $scope, $uibModalInstance, committeeRepository, pictureRepository,
        shroomsFileUploader, notifySrv, $uibModal, imageValidationSettings, committeeImageConfig, authService) {

        $scope.hasPermissions = authService.hasPermissions(['COMMITTEES_ADMINISTRATION']);

        $scope.attachedFiles = [];

        $scope.thumbHeight = committeeImageConfig.thumbHeight;
        $scope.thumbWidth = committeeImageConfig.thumbWidth;

        $scope.allUsers = function (search) {
            return committeeRepository.getUsers(search);
        }

        $scope.imageAttached = function (input) {
            var isValid;

            if (input.value) {
                isValid = shroomsFileUploader.validate(
                    input.files,
                    imageValidationSettings,
                    showUploadAlert
                    );
            }

            if (isValid) {
                $scope.attachedFiles = shroomsFileUploader.fileListToArray(input.files);
                $scope.committeePicture = $scope.attachedFiles[0];
            }

            $scope.$apply();
            input.value = '';
        }

        $scope.submitNewCommittee = function (committee) {
            if ($scope.attachedFiles.length) {
                pictureRepository.upload($scope.attachedFiles).then(function (response) {
                    committee.pictureId = response.data;
                    saveCommittee(committee);
                });
            } else {
                saveCommittee(committee);
            }
        }

        function saveCommittee (committee) {
            if ($scope.edit) {
                committeeRepository.put(committee).then(onSuccess, onError);
            } else {
                committeeRepository.post(committee).then(onSuccess, onError);
            }
        }

        $scope.cancel = function () {
            $uibModalInstance.close();
            $scope.getCommitteesList($scope.filter);
        };

        function onSuccess() {
            notifySrv.success('common.infoSaved');
            $uibModalInstance.close();
            $scope.getCommitteesList($scope.filter);
            $state.reload();
        };

        function onError(response) {
            notifySrv.error(response.data);
        };

        function showUploadAlert(status) {
            var statuses = {
                success: success,
                sizeError: sizeError,
                typeError: typeError
            }

            if (statuses.hasOwnProperty(status)) {
                statuses[status]();
            }

            function success() {
                notifySrv.success('committee.imageAttached');
            }

            function sizeError() {
                notifySrv.error('committee.imageSizeExceeded');
            }

            function typeError() {
                notifySrv.error('committee.imageInvalidType');
            }
        }

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
