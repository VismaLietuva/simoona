(function () {
    "use strict";

    angular.module('simoonaApp.Office.Floor')
        .controller('floorManageController', floorManageController);

    floorManageController.$inject = [
        '$scope',
        '$rootScope',
        '$state',
        'floorRepository',
        'model',
        'localStorageService',
        '$translate',
        'notifySrv',
        'localeSrv',
        'shroomsFileUploader',
        'errorHandler',
        'pictureRepository'
    ];

    function floorManageController($scope, $rootScope, $state, floorRepository, model,
     localStorageService, $translate, notifySrv, localeSrv, shroomsFileUploader, errorHandler, pictureRepository) {

        var authData = localStorageService.get('authorizationData');

        $scope.pictureUrl = '';
        $scope.attachImage = attachImage;

        $scope.previewImage = {
            attachedFiles : [],
            imageSource : ''
        };

        $scope.state = {
            edit: $state.current.name === 'Root.WithOrg.Admin.Offices.Floors.Edit',
            create: $state.current.name === 'Root.WithOrg.Admin.Offices.Floors.Create'
        };

        $scope.isPlanInvalid = false;

        function attachImage(input) {
            var imageValidationSettings = {
                allowed: ['image/png', 'image/jpg', 'image/jpeg', 'image/gif'],
                sizeLimit: 52428800
            };

            if (input.value) {
                if (shroomsFileUploader.validate(input.files, imageValidationSettings, showUploadAlert)) {
                    $scope.isPictureSelected = true;
                    $scope.previewImage.attachedFiles = shroomsFileUploader.fileListToArray(input.files);
                    var reader = new FileReader();
                    reader.readAsDataURL($scope.previewImage.attachedFiles[0]);
                    reader.onload = function () {
                        $scope.previewImage.imageSource = reader.result;
                        $scope.$apply();
                    };
                }
            }
            input.value = '';
        }

        function showUploadAlert(status) {
            if (status === 'sizeError') {
                notifySrv.error('wall.imageSizeExceeded');
            } else if (status === 'typeError') {
                notifySrv.error('wall.imageInvalidType');
            }
        }

        var createFloor = function (pictureId) {
            if (pictureId) {
                $scope.floor.pictureId = pictureId;
            }

            floorRepository.create($scope.floor).then(
                function (response) {
                    $state.go('^.List');
                    notifySrv.success(localeSrv.formatTranslation('common.messageEntityCreated', {one: 'floor.entityName', two: $scope.floor.name}));
                },
                function (response) {
                    $scope.errors = response;
                });
        };

        var uploadFloor = function (pictureId) {
            if (pictureId) {
                $scope.floor.pictureId = pictureId;
            }

            floorRepository.update($scope.floor).then(
                function () {
                    $state.go('^.List');
                    var alertMessage = localeSrv.formatTranslation('common.messageEntityChanged', {one:'floor.entityName', two: $scope.floor.name});
                    notifySrv.success(alertMessage);
                },
                function (response) {
                    $scope.errors = response;
                });
        };

        $rootScope.pageTitle = $scope.state.create ? 'floor.createFloor' : 'floor.editFloor';

        $scope.floor = model;
        $scope.isPictureSelected = false;

        if ($scope.state.create) {
            $scope.floor.officeId = $state.params.officeId;
        }

        $scope.create = function () {
            $scope.errors = [];

            if ($scope.previewImage.attachedFiles.length) {
                pictureRepository.upload($scope.previewImage.attachedFiles).then(function (promise) {
                    createFloor(promise.data);
                }, errorHandler.handleErrorMessage);
            }
        };

        $scope.update = function () {
            $scope.errors = [];
            if ($scope.previewImage.attachedFiles.length) {
                pictureRepository.upload($scope.previewImage.attachedFiles).then(function (promise) {
                    uploadFloor(promise.data);
                }, errorHandler.handleErrorMessage);
            } else {
                uploadFloor();
            }
        };

        $scope.openPictureModal = function (pictureId) {
            $uibModal.open({
                templateUrl: 'app/picture/picture-modal/picture-modal-content.html',
                size: 'lg',
                controller: 'pictureModalController',
                resolve: {
                    pictureId: function () {
                        return pictureId;
                    }
                }
            });
        };
    }
})();
