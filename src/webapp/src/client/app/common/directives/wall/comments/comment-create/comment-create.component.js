(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceWallCommentCreate', {
            bindings: {
                wallId: '=',
                isWallModule: '=',
                post: '='
            },
            templateUrl: 'app/common/directives/wall/comments/comment-create/comment-create.html',
            controller: wallPostCommentCreateController,
            controllerAs: 'vm'
        });

    wallPostCommentCreateController.$inject = [
        '$scope',
        '$element',
        'imageValidationSettings',
        'shroomsFileUploader',
        'pictureRepository',
        'wallImageConfig',
        'wallCommentRepository',
        'wallSettings',
        'wallService',
        'errorHandler',
        'notifySrv',
        'dataHandler',
        'employeeListRepository'
    ];

    function wallPostCommentCreateController($scope, $element, imageValidationSettings, shroomsFileUploader,
        pictureRepository, wallImageConfig, wallCommentRepository, wallSettings, wallService, errorHandler, notifySrv, dataHandler, employeeListRepository) {
        /*jshint validthis: true */
        var vm = this;

        vm.searchPosition = 0;
        vm.onSearch = onSearch;
        vm.getEmployeeList = getEmployeeList;
        vm.hasChanged = hasChanged;
        vm.employeeList = [];
        vm.showSubmit = showSubmit;
        vm.attachImage = attachImage;
        vm.submitComment = submitComment;
        vm.isSubmittable = isSubmittable;
        vm.handleFormSubmit = handleFormSubmit;
        vm.handleErrorMessage = handleErrorMessage;
        vm.employeeListRepository = employeeListRepository;
        $scope.pagedEmployeeList = {};

        vm.commentForm = {};
        vm.attachedFiles = [];
        vm.isFormEnabled = true;
        vm.showSubmitButton = false;
        vm.isSearchingEmployee = false;
        vm.maxLength = wallSettings.postMaxLength;
        vm.thumbHeight = wallImageConfig.thumbHeight;

        init();

        //////////

        function init() {
            $scope.$on('focusReplyField', function () {
                vm.showSubmitButton = true;
                $element.find('textarea').focus();
            });
        }

        function isSubmittable() {
            if (!!vm.commentForm.messageBody) {
                return !!vm.commentForm.messageBody.trim().length && vm.isFormEnabled;
            }
        }

        function submitComment() {
            if (isSubmittable()) {
                vm.isFormEnabled = false;

                if (vm.attachedFiles.length) {
                    pictureRepository.upload(vm.attachedFiles).then(function (promise) {
                        handleFormSubmit(promise.data);
                    }, vm.handleErrorMessage);
                } else {
                    handleFormSubmit();
                }
            }
        }

        function handleFormSubmit(pictureId) {
            vm.commentForm.postId = vm.post.id;
            vm.commentForm.pictureId = pictureId;

            wallCommentRepository.createComment(vm.commentForm).then(function() {
                wallService.initWall(vm.isWallModule, vm.wallId);
            }, errorHandler.handleErrorMessage);

            clearComment();
        }

        function handleErrorMessage(errorResponse) {
            vm.isFormEnabled = true;

            errorHandler.handleErrorMessage(errorResponse);
        }

        function showSubmit() {
            vm.showSubmitButton = true;
        }

        function clearComment() {
            vm.commentForm = {};
            vm.attachedFiles = [];
            vm.isFormEnabled = true;
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
            input.value = '';
        }

        function showUploadAlert(status) {
            if (status === 'sizeError') {
                notifySrv.error('wall.imageSizeExceeded');
            } else if (status === 'typeError') {
                notifySrv.error('wall.imageInvalidType');
            }
            $scope.$apply();
        }

        function onSearch () {

            if (vm.commentForm.messageBody[vm.commentForm.messageBody.length - 1] === '@') {
                vm.isSearchingEmployee = true;
                vm.searchPosition = vm.commentForm.messageBody.length;
            }

            if (vm.isSearchingEmployee) {
                $scope.filter = {
                    page: 1,
                    search: vm.commentForm.messageBody.substr(vm.searchPosition)
                };
                vm.getEmployeeList();
            }
        };

        function getEmployeeList () {
            employeeListRepository.getPaged($scope.filter).then(function (getPagedResponse) {
                vm.employeeList = getPagedResponse.pagedList;

                console.log(vm.employeeList);
                if (vm.employeeList.length > 1) {
                    vm.selected = vm.employeeList[0];
                }
            });
        };

        function hasChanged() {
            if (vm.selected) {
                vm.commentForm.messageBody += vm.selected.firstName + ' ' + vm.selected.lastName + ' ';
                vm.isSearchingEmployee = false;
            }
        }
    }
}());
