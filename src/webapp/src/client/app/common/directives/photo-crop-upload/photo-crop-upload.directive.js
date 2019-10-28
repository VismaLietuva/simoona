(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('acePhotoCropUpload', photoCropUpload);

    function photoCropUpload() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/photo-crop-upload/photo-crop-upload.html',
            scope: {
                isCropVisible: '=',
                aspectRatio: '=',
                resultImage: '=',
                imageSize: '=',
                multiple: '=',
                images: "=?"
            },
            controller: photoCropUploadController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;

    }

    photoCropUploadController.$inject = [
        '$scope',
        'shroomsFileUploader',
        'imageValidationSettings',
        'notifySrv'
    ];

    function photoCropUploadController($scope, shroomsFileUploader, imageValidationSettings, notifySrv) {
        /* jshint validthis: true */
        var vm = this;
        vm.images = [];
        $scope.handleFileSelect = handleFileSelect;
        ///////

        function handleFileSelect(files) {
            if (imageAttached(files)) {
                if (!$scope.multiple) {
                    vm.images = [];
                }
                Object.keys(files).forEach((photo, index) => {
                    vm.images.push(files[photo]);
                });

                vm.images.forEach((image) => {
                    var reader = new FileReader();
                    reader.onload = function (files) {
                        $scope.$apply();
                    };
                    reader.readAsDataURL(image)
                });
            }

        }

        function imageAttached(input) {
            var isValid;
            if (!input) {
                input = {};
            }

            if (!!input.length) {
                isValid = shroomsFileUploader.validate(
                    input,
                    imageValidationSettings,
                    showUploadAlert
                );
                if (!isValid) {
                    input = [];
                }
            }

            $scope.$apply();

            return isValid;
        }

        function showUploadAlert(status) {
            var statuses = {
                sizeError: sizeError,
                typeError: typeError
            };

            if (statuses.hasOwnProperty(status)) {
                statuses[status]();
            }

            function sizeError() {
                notifySrv.error('picture.imageSizeExceeded');
            }

            function typeError() {
                notifySrv.error('picture.imageInvalidType');
            }
        }

    }
})();
