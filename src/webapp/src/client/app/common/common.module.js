(function() {
    'use strict';

    angular
        .module('simoonaApp.Common', [
            'toaster',
            'ngLodash',
            'ngResource',
            'angularMoment',
            'bootstrapLightbox',
            'LocalStorageModule',
            'pascalprecht.translate',
            'simoonaApp.Constant',
            'simoonaApp.Common.ConfirmationPopup'
        ])
        .config(config);

    config.$inject = [
        '$provide'
    ];

    function config($provide) {
        // Decorator resets images on opening modal
        $provide.decorator('Lightbox', ['$delegate', '$uibModal',
            function LightboxDecorator($delegate, $uibModal) {
                $delegate.openModal = function (newImages, newIndex, modalParams) {
                    $delegate.index = 1;
                    $delegate.image = {};
                    $delegate.imageUrl = null;
                    $delegate.imageCaption = null;
                    $delegate.keyboardNavEnabled = false;

                    $delegate.images = newImages;
                    $delegate.setImage(newIndex);

                    // store the modal instance so we can close it manually if we need to
                    $delegate.modalInstance = $uibModal.open(angular.extend({
                        'templateUrl': 'app/common/directives/lightbox/lightbox.html',
                        'controller': ['$scope', function ($scope) {
                            $scope.Lightbox = $delegate;

                            $delegate.keyboardNavEnabled = true;
                        }],
                        'windowClass': 'lightbox-modal'
                    }, modalParams || {}));

                    return $delegate.modalInstance;
                };

                return $delegate;
            }
        ]);

        $provide.decorator('fileUploaderOptions', ['$delegate', 'localStorageService', 'endPoint',
            function (fileUploaderOptions, localStorageService, endPoint) {
                var authData = localStorageService.get('authorizationData');
                if (authData) {
                    fileUploaderOptions.headers = {
                        Organization: authData.organizationName,
                        Authorization: 'Bearer ' + authData.token
                    };
                    fileUploaderOptions.url = endPoint + '/Picture/Upload';

                    return fileUploaderOptions;
                }
            }
        ]);
    }
})();
