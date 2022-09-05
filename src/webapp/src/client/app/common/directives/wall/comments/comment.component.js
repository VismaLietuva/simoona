(function () {
    'use strict';

    angular.module('simoonaApp.Common').component('aceWallComment', {
        replace: true,
        bindings: {
            comment: '=',
            isAdmin: '=',
            wallId: '=',
            isWallModule: '=',
            hasHashtagify: '=',
            isHidden: '<',
        },
        templateUrl: 'app/common/directives/wall/comments/comment.html',
        controller: wallCommentController,
        controllerAs: 'vm',
    });

    wallCommentController.$inject = [
        'wallSettings',
        'errorHandler',
        'youtubeSettings',
        'wallCommentRepository',
        'wallService',
        'lodash',
    ];

    function wallCommentController(
        wallSettings,
        errorHandler,
        youtubeSettings,
        wallCommentRepository,
        wallService,
        lodash
    ) {
        /*jshint validthis: true */
        var vm = this;

        vm.enableEditor = enableEditor;
        vm.disableEditor = disableEditor;
        vm.editComment = editComment;
        vm.deleteComment = deleteComment;
        vm.handleErrorMessage = handleErrorMessage;

        vm.isActionsEnabled = true;
        vm.editFieldEnabled = false;
        vm.maxLength = wallSettings.postMaxLength;
        vm.youtubeWidth = youtubeSettings.width;
        vm.youtubeHeight = youtubeSettings.height;
        vm.youtubePreviewWidth = youtubeSettings.previewWidth;
        vm.youtubePreviewHeight = youtubeSettings.previewHeight;
        vm.singlePictureId = lodash.first(vm.comment.images);

        //////////

        function editComment(messageBody) {
            if (vm.isActionsEnabled) {
                vm.disableEditor();

                vm.comment.messageBody = messageBody;
                vm.isActionsEnabled = false;

                wallCommentRepository.editComment(vm.comment).then(function () {
                    vm.isActionsEnabled = true;
                    wallService.initWall(vm.isWallModule, vm.wallId);
                }, vm.handleErrorMessage);
            }
        }

        function deleteComment() {
            if (vm.isActionsEnabled) {
                vm.disableEditor();

                vm.isActionsEnabled = false;

                wallCommentRepository
                    .deleteComment(vm.comment)
                    .then(function () {
                        vm.isActionsEnabled = true;
                        wallService.initWall(vm.isWallModule, vm.wallId);
                    }, vm.handleErrorMessage);
            }
        }

        function handleErrorMessage(response) {
            vm.isActionsEnabled = true;
            errorHandler.handleErrorMessage(response);
        }

        function enableEditor() {
            vm.editFieldEnabled = true;
            vm.editableValue = vm.comment.messageBody;
        }

        function disableEditor() {
            vm.editFieldEnabled = false;
        }
    }
})();
