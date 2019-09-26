(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .constant('kudosifySettings', {
            KudosifyTypes: {
                0: 'submit',
                1: 'send'
            },
            maxMinus: 99999
        })
        .directive('aceKudosifyModal', kudosifyModal);

    kudosifyModal.$inject = [
        '$uibModal'
    ];

    function kudosifyModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceKudosifyModal: '=?'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, elem, attrs) {
            elem.bind('click', function () {
                $uibModal.open({
                    templateUrl: 'app/kudos/kudosify-modal/kudosify-modal.html',
                    controller: kudosifyModalController,
                    controllerAs: 'vm',
                    resolve: {
                        currentUser: function () {
                            return scope.aceKudosifyModal;
                        },
                        context: function() {
                            return attrs.kudosifyType;
                        }
                    }
                });
            });
        }
    }

    kudosifyModalController.$inject = [
        '$scope',
        '$uibModalInstance',
        'authService',
        'kudosifyModalFactory',
        'kudosFactory',
        'notifySrv',
        'kudosifySettings',
        'currentUser',
        'context',
        'imageValidationSettings',
        'shroomsFileUploader',
        'pictureRepository',
        'lodash',
        'dataHandler',
        'errorHandler'
    ];

    function kudosifyModalController($scope, $uibModalInstance, authService, kudosifyModalFactory, kudosFactory,
        notifySrv, kudosifySettings, currentUser, context, imageValidationSettings, shroomsFileUploader,
        pictureRepository, lodash, dataHandler, errorHandler) {
        /*jshint validthis: true */

        var vm = this;
        vm.submitKudos = submitKudos;
        vm.cancelKudos = cancelKudos;
        vm.setModalType = setModalType;
        vm.getUsers = getUsersForAutocomplete;
        vm.attachImage = attachImage;

        vm.userId = authService.identity.userId;
        vm.context = context;

        vm.pointsType = {};
        vm.kudosifyInfo = {};
        vm.kudosReceivers = [];
        vm.isButtonSelected = false;
        vm.lastSelectedButton = undefined;
        vm.isBusy = false;

        vm.attachedFiles = [];
        vm.thumbHeight = 300;

        vm.maxMinus = kudosifySettings.maxMinus;

		var KudosTypesEnum = {
			ORDINARY: 1,
			SEND: 2,
			MINUS: 3,
			OTHER: 4
        };
        
        init();

        //////

        function init() {
            if (vm.context === 'submit')
            {
                kudosifyModalFactory.getPointsTypes().then(function (result) {
                    vm.kudosTypes = result;
                });
            }
            else if (vm.context === 'send')
            {
                vm.kudosTypes = [ 'send' ];
            }

            kudosFactory.getUserInformation(vm.userId).then(function (response) {
                vm.user = response;
                vm.kudosifyUser = {
                    formattedName: response.firstName + ' ' + response.lastName,
                    id: vm.userId
                };
                vm.isLoading = false;
            });

            if (currentUser) {
                vm.kudosReceivers.push(currentUser);
            }
        }

        function setModalType(type, dom) {
            if (!dom.target.classList.contains('kudosify-modal-buttons-inactive') && vm.isButtonSelected) {
                vm.kudosifyInfo.multiplyBy++;
            } else {
                vm.isButtonSelected = true;
                vm.kudosifyInfo.multiplyBy = 1;
                var buttons = document.getElementsByClassName('kudos-type-button');

                for (var i = 0; i < buttons.length; i++) {
                    buttons[i].classList.add('kudosify-modal-buttons-inactive');
                }

                dom.target.classList.remove('kudosify-modal-buttons-inactive');
                vm.pointsType = type;
                recalculateTotalPoints();
            }
        }

        function recalculateTotalPoints(){
            vm.kudosifyInfo.totalPoints = vm.kudosifyInfo.multiplyBy * vm.pointsType.value;
        }

        function getUsersForAutocomplete(query) {
            return kudosFactory.getUsersForAutoComplete(query);
        }

        function submitKudos(kudosReceivers, kudosifyInfo, pointsType) {
            vm.isBusy = true;
            kudosReceivers = lodash.map(kudosReceivers, 'id');

            if (vm.attachedFiles.length) {
                pictureRepository.upload(vm.attachedFiles).then(function(result) {
                    kudosifyInfo.imageName = result.data;
                    postKudos(kudosReceivers, kudosifyInfo, pointsType);
                });
            }
            else{
                kudosifyInfo.imageName = null;
                postKudos(kudosReceivers, kudosifyInfo, pointsType);
            }
        }

        function postKudos(kudosReceivers, kudosifyInfo, pointsType){
            kudosifyModalFactory.postKudos(kudosReceivers, kudosifyInfo, pointsType).then(function (result) {
                notifySrv.success('common.successfullySaved');
                $uibModalInstance.close();
            }, function (error) {
                errorHandler.handleErrorMessage(error);
                vm.isBusy = false;
            });
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

        function cancelKudos() {
            $uibModalInstance.dismiss('cancel');
        }
    }
})();
