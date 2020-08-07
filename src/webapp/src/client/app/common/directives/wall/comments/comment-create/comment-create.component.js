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
        'kudosFactory',
        '$translate',
        ];

    function wallPostCommentCreateController($scope, $element, imageValidationSettings, shroomsFileUploader,
        pictureRepository, wallImageConfig, wallCommentRepository, wallSettings, wallService, errorHandler, notifySrv, dataHandler, kudosFactory, translate) {
        /*jshint validthis: true */
        var vm = this;
        vm.showSubmit = showSubmit;
        vm.attachImage = attachImage;
        vm.submitComment = submitComment;
        vm.isSubmittable = isSubmittable;
        vm.handleFormSubmit = handleFormSubmit;
        vm.handleErrorMessage = handleErrorMessage;

        vm.selectedMentions = [];
        vm.commentForm = {};
        vm.attachedFiles = [];
        vm.isFormEnabled = true;
        vm.showSubmitButton = false;
        vm.isSearchingEmployee = false;
        vm.maxLength = wallSettings.postMaxLength;
        vm.thumbHeight = wallImageConfig.thumbHeight;
        vm.getUsersForAutocomplete = getUsersForAutocomplete;
        vm.invokeMention = invokeMention;
        vm.selectMention = selectMention;

        init();

        //////////

        function init() {
            $scope.$on('focusReplyField', function () {
                vm.showSubmitButton = true;
                $element.find('textarea').focus();
            });

            vm.addComment = translate.instant('wall.addComment');
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

        function selectMention(item) {
            vm.selectedMentions.push({id: item.id, fullName: item.label});

            return `**@${item.label.replace(' ', '_')}**`;
        }

        function invokeMention(term) {
            if (term) {
                getUsersForAutocomplete(term).then(function(response) {
                    vm.employees = response.map(function(cur) {
                        return {
                            id: cur.id,
                            label: cur.formattedName
                        }
                    });
                });
            }
        }

        function handleFormSubmit(pictureId) {
            vm.commentForm.postId = vm.post.id;
            vm.commentForm.pictureId = pictureId;
            vm.commentForm.mentions = compareAndGetMentions()

            wallCommentRepository.createComment(vm.commentForm).then(function() {
                wallService.initWall(vm.isWallModule, vm.wallId);
            }, errorHandler.handleErrorMessage);

            clearComment();
        }


        function compareAndGetMentions() {
            var parsedNamesFromTextBody = parseMentions(vm.commentForm.messageBody);

            return vm.selectedMentions.filter(function(cur) {
                if(parsedNamesFromTextBody.includes(cur.fullName)) {
                    return cur;
                }
            });
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

        function getUsersForAutocomplete(query) {
            return kudosFactory.getUsersForAutoComplete(query);
        }
        
        function parseMentions (text) {
            var pattern = /\B@[a-z0-9_-]+/gi;

            return text.match(pattern).map(cur => {
                return cur.replace('@', '')
                         .replace('_', ' ');
            });
        }
    }
}());
