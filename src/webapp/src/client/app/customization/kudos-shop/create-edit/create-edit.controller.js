(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.KudosShop')
        .controller('kudosShopCreateController', kudosShopCreateController);

    kudosShopCreateController.$inject = [
        '$rootScope',
        '$scope',
        '$state',
        '$stateParams',
        'kudosShopRepository',
        'errorHandler',
        'imageValidationSettings', 
        'shroomsFileUploader',
        'pictureRepository',
        'dataHandler'
    ];

    function kudosShopCreateController($rootScope, $scope, $state, $stateParams, kudosShopRepository, errorHandler, imageValidationSettings, 
        shroomsFileUploader, pictureRepository, dataHandler) {
        var vm = this;
        var listState = 'Root.WithOrg.Admin.Customization.KudosShop.List';

        vm.kudosShopItem = {};
        vm.onEditOriginalName = '';
        vm.states = {
            isAdd: $state.includes('Root.WithOrg.Admin.Customization.KudosShop.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Customization.KudosShop.Edit')
        };
        vm.isLoading = vm.states.isAdd ? false : true;

        $rootScope.pageTitle = vm.states.isAdd ? 'kudosShop.createKudosShopItem' : 'kudosShop.editKudosShopItem';

        vm.createKudosShopItem = createKudosShopItem;
        vm.updateKudosShopItem = updateKudosShopItem;
        vm.saveKudosShopItem = saveKudosShopItem;
        vm.deleteKudosShopItem = deleteKudosShopItem;

        vm.attachImage = attachImage;
        vm.attachedFiles = [];

        init();

        ///////////

        function init() {
            if ($stateParams.id) {
                kudosShopRepository.getKudosShopItem($stateParams.id).then(function (response) {
                    vm.kudosShopItem = response;
                    vm.onEditOriginalName = response.name;
                    vm.isLoading = false;
                }, function (error) {
                    errorHandler.handleErrorMessage(error);
                    $state.go(listState);
                });
            }
        }

        function createKudosShopItem() {
            return kudosShopRepository.createKudosShopItem(vm.kudosShopItem);
        }

        function updateKudosShopItem() {
            return kudosShopRepository.updateKudosShopItem(vm.kudosShopItem);
        }

        function saveKudosShopItem(method) {
            if (vm.attachedFiles.length) {
                pictureRepository.upload(vm.attachedFiles).then(function(result) {
                    vm.kudosShopItem.pictureId = result.data;
                    method(vm.kudosShopItem).then(function () {
                        $state.go(listState);
                    }, errorHandler.handleErrorMessage);
                });
            }
            else{
                vm.kudosShopItem.pictureId = null;
                method(vm.kudosShopItem).then(function () {
                    $state.go(listState);
                }, errorHandler.handleErrorMessage);
            }
        }

        function deleteKudosShopItem(id) {
            kudosShopRepository.deleteKudosShopItem(id).then(function() {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
        }

        function attachImage(input) {
            var options = { 
                canvas: true
            };

            if (input.value) {
                if (shroomsFileUploader.validate(input.files, imageValidationSettings, showUploadAlert)) {
                    vm.attachedFiles = shroomsFileUploader.fileListToArray(input.files);
                    var displayImg = function(img) {
                        $scope.$apply(function($scope) {
                            var fileName = vm.attachedFiles[0].name;

                            vm.imageSource = img.toDataURL(vm.attachedFiles[0].type);
                            if (vm.attachedFiles[0].type !== 'image/gif') {
                                vm.attachedFiles[0] = dataHandler.dataURItoBlob(vm.imageSource, vm.attachedFiles[0].type);
                                vm.attachedFiles[0].name = fileName;
                            }
                        });
                    };

                    loadImage.parseMetaData(vm.attachedFiles[0], function (data) {
                        if (data.exif) {
                            options.orientation = data.exif.get('Orientation');
                        }

                        loadImage(vm.attachedFiles[0], displayImg, options);
                    });
                }
            }
        }

        function showUploadAlert(status) {
            if (status === 'sizeError') {
                notifySrv.error('wall.imageSizeExceeded');
            } else if (status === 'typeError') {
                notifySrv.error('wall.imageInvalidType');
            }
            $scope.$apply();
        }

    }
})();