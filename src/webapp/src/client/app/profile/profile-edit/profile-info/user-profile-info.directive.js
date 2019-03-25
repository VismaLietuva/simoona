(function () {
    'use strict';

    angular
        .module('simoonaApp.Profile')
        .constant('profileImageConfig', {
            thumbHeight: 200,
            thumbWidth: 200
        })
        .constant('dateSettings', {
            minDate: new Date('1900-01-01'),
            maxDate: new Date()
        })
        .directive('profileInfo', profileInfo);

    function profileInfo() {

        return {
            templateUrl: 'app/profile/profile-edit/profile-info/user-profile-info.html',
            restrict: 'AE',
            scope: {
                model: '='
            },
            controller: controller
        }
    }

    controller.$inject = [
        '$scope',
        '$state',
        '$timeout',
        'authService',
        'notifySrv',
        'userRepository',
        '$http',
        'profileRepository',
        'pictureRepository',
        'shroomsFileUploader',
        'dateSettings',
        'appConfig',
        'imageValidationSettings',
        'profileImageConfig',
        'endPoint',
        'localStorageService',
        'dataHandler'
    ];

    function controller($scope, $state, $timeout, authService, notifySrv, userRepository, $http, profileRepository,
        pictureRepository, shroomsFileUploader, dateSettings, appConfig,
        imageValidationSettings, profileImageConfig, endPoint, localStorageService, dataHandler) {

        $scope.postForm = {};
        $scope.thumbHeight = profileImageConfig.thumbHeight;
        $scope.thumbWidth = profileImageConfig.thumbWidth;
        $scope.attachedFiles = [];
        $scope.imageAttached = imageAttached;
        $scope.isNewUser = authService.identity.isAuthenticated && authService.isInRole('NewUser');
        $scope.canEditUserProfiles = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);

        $scope.errors = [];
        $scope.infos = [];

        $scope.info = $scope.model;
        $scope.dateSettings = dateSettings;

        if ($scope.model.birthDay) {
            $scope.info.birthDay = new Date($scope.info.birthDay);
        } else {
            $scope.info.birthDay = new Date("1993");
        }

        $scope.profilePicture = $scope.model.pictureId;

        $scope.datePickers = {
            birthDay: false
        };

        $scope.openDatePicker = openDatePicker;

        $scope.birthdayDatepickerOpened = true;
        $scope.toggleDatePicker = toggleDatePicker;

        var putPersonalInfo = function (pictureId) {
            if (pictureId) {
                $scope.info.pictureId = pictureId;
            }

            var model = angular.copy($scope.info);

            profileRepository.putPersonalInfo(model).then(onSuccess, onError);

            function onSuccess(response) {
                if ($scope.isNewUser && authService.isInRole('FirstLogin')) {
                    if (!response.requiresConfirmation) {
                        var authData = localStorageService.get('authorizationData');

                        authService.getUserInfo(authData.token).then(function(response) {
                            authService.setAuthenticationData(response, authData.token);

                            $state.go(appConfig.homeStateName, {
                                organizationName: authService.identity.organizationName
                            }, {
                                reload: true
                            });
                        });
                    }
                    else {
                        $state.reload();
                        notifySrv.success('common.infoSavedAdminNotified');
                    }
                } else {
                    $state.reload();
                    notifySrv.success('common.infoSaved');
                }
            }

            function onError(response) {
                notifySrv.error('common.invalidData');
            }
        };

        $scope.saveInfo = function () {
            $scope.errors = [];
            $scope.infos = [];

            if ($scope.attachedFiles.length) {
                pictureRepository.upload($scope.attachedFiles).then(function (promise) {
                    putPersonalInfo(promise.data);
                });
            } else {
                putPersonalInfo();
            }
        };

        function toggleDatePicker(key, $event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.datePickers[key] = true;
        }

        function imageAttached(input) {
            var isValid;

            if (input.value) {
                isValid = shroomsFileUploader.validate(
                    input.files,
                    imageValidationSettings,
                    showUploadAlert
                    );
            }

            if (isValid) {
                var options = { 
                    canvas: true
                };
                $scope.attachedFiles = shroomsFileUploader.fileListToArray(input.files);
                var displayImg = function(img) {
                    var fileName = $scope.attachedFiles[0].name;
                    $scope.$apply(function($scope) {
                        $scope.imageSource = img.toDataURL($scope.attachedFiles[0].type);
                    });
                    if ($scope.attachedFiles[0].type !== 'image/gif') {
                        $scope.attachedFiles[0] = dataHandler.dataURItoBlob($scope.imageSource, $scope.attachedFiles[0].type);
                        $scope.attachedFiles[0].name = fileName;
                    }
                };

                loadImage.parseMetaData($scope.attachedFiles[0], function (data) {
                    if (data.exif) {
                        options.orientation = data.exif.get('Orientation');
                    }

                    loadImage($scope.attachedFiles[0], displayImg, options);
                });

                $scope.profilePicture = $scope.attachedFiles[0];
            }

            $scope.$apply();

            input.value = '';
        }

        function showUploadAlert(status) {
            var statuses = {
                success: success,
                sizeError: sizeError,
                typeError: typeError
            };

            if (statuses.hasOwnProperty(status)) {
                statuses[status]();
            }

            function success() {
                notifySrv.success('picture.imageAttached');
            }

            function sizeError() {
                notifySrv.error('picture.imageSizeExceeded');
            }

            function typeError() {
                notifySrv.error('picture.imageInvalidType');
            }
        }

        $scope.onChangeValidateBirthDay = function ()
        {
            if ($scope.info.birthDay) {
                $scope.personalInfo.birthDay.$setValidity('minDate', $scope.info.birthDay >= convertDateForBackendUtc(dateSettings.minDate));
                $scope.personalInfo.birthDay.$setValidity('maxDate', $scope.info.birthDay <= convertDateForBackendUtc(dateSettings.maxDate));
            }
        };

        function convertDateForBackendUtc(datetime) {
            return new Date(datetime.getTime() + (datetime.getTimezoneOffset() * 60000)); //temp fix
        }

        function openDatePicker($event, key) {
            $event.preventDefault();
            $event.stopPropagation();

            closeAllDatePickers(key);

            $timeout(function() {
                $event.target.focus();
            }, 100);
        }

        function closeAllDatePickers(datePicker) {
            $scope.datePickers.birthDay = false;
            
            $scope.datePickers[datePicker] = true;
        }
    }
})();
