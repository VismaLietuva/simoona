(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceWallPost', {
            replace: true,
            bindings: {
                post: '=',
                isAdmin: '=',
                hasHashtagify: '=',
                isCollapsed: '=',
                wallId: '=',
                isWallModule: '=',
                index: '='
            },
            templateUrl: 'app/common/directives/wall/posts/post.html',
            controller: wallPostController,
            controllerAs: 'vm'
        });

    wallPostController.$inject = [
        '$scope',
        '$state',
        '$location',
        'SmoothScroll',
        'wallSettings',
        'errorHandler',
        'youtubeSettings',
        'notificationFactory',
        'wallPostRepository',
        'wallService',
        'notifySrv',
        '$window'
    ];

    function wallPostController($scope, $state, $location, SmoothScroll, wallSettings,
        errorHandler, youtubeSettings, notificationFactory, wallPostRepository, wallService, notifySrv, $window) {
        /*jshint validthis: true */
        var vm = this;

        vm.isSeen = isSeen;
        vm.editPost = editPost;
        vm.deletePost = deletePost;
        vm.enableEditor = enableEditor;
        vm.disableEditor = disableEditor;
        vm.showCommentForm = showCommentForm;
        vm.handleErrorMessage = handleErrorMessage;

        vm.notifications = notificationFactory.notification;
        vm.markAsRead = notificationFactory.markAsRead;

        vm.isActionsEnabled = true;
        vm.editFieldEnabled = false;
        vm.maxLength = wallSettings.postMaxLength;
        vm.youtubeWidth = youtubeSettings.width;
        vm.youtubeHeight = youtubeSettings.height;
        vm.youtubePreviewWidth = youtubeSettings.previewWidth;
        vm.youtubePreviewHeight = youtubeSettings.previewHeight;
        vm.stateParams = $state.params;
        vm.usePostWatching = $window.usePostWatching;

        vm.getPostUrl = getPostUrl;
        vm.notifyCopied = notifyCopied;
        vm.watchPost = watchPost;
        vm.unwatchPost = unwatchPost;

        init();
        
        /////////

        function init()
        {
            isSeen(vm.post.id);
        }

        function isSeen(postId) {
            if (!!vm.stateParams.post) {
                var notificationIds = [];
                angular.forEach(vm.notifications.data, (notification) => {
                    if (postId === notification.sourceIds.postId) {
                        notificationIds.push(notification.id);
                    }
                });
                if(!!notificationIds.length)
                {
                    vm.markAsRead(notificationIds);
                    notificationIds = [];
                }
            }
        }
        function editPost(messageBody) {
            if (vm.isActionsEnabled) {
                vm.disableEditor();

                vm.post.messageBody = messageBody;
                vm.isActionsEnabled = false;

                wallPostRepository.editPost(vm.post).then(function() {
                    vm.isActionsEnabled = true;
                    wallService.initWall(vm.isWallModule, vm.wallId);
                }, vm.handleErrorMessage);
            }
        }

        function deletePost() {
            if (vm.isActionsEnabled) {
                vm.disableEditor();

                vm.isActionsEnabled = false;

                wallPostRepository.deletePost(vm.post).then(function() {
                    vm.isActionsEnabled = true;
                    wallService.initWall(vm.isWallModule, vm.wallId);
                }, vm.handleErrorMessage);
            }
        }

        function showCommentForm(id) {
            if (!vm.isCollapsed) {
                var hash = 'comment-create-anchor-' + id;
                SmoothScroll.scrollTo(hash, getReplyAnchorOffset(hash));
            }

            $scope.$parent.$broadcast('focusReplyField');
        }

        function getReplyAnchorOffset(hash) {
            var offset = window.innerHeight / 2;
            var anchoredElement = document.getElementById(hash);
            if (anchoredElement) {
                offset -= anchoredElement.clientHeight / 2;
            }

            return offset;
        }

        function handleErrorMessage(response) {
            vm.isActionsEnabled = true;

            errorHandler.handleErrorMessage(response);
        }

        function enableEditor() {
            vm.editFieldEnabled = true;
            vm.editableValue = vm.post.messageBody;
        }

        function disableEditor() {
            vm.editFieldEnabled = false;
        }
        
        function getPostUrl(id) {
            if ($state.includes('Root.WithOrg.Client.Events') || $state.includes('Root.WithOrg.Client.Projects')){
                return $location.absUrl();
            } else if ($state.includes('Root.WithOrg.Client.Wall.All')){
                return $location.absUrl().split('?')[0].replace('/All', '/Feed') + '?post=' + id;
            } else {
                return $location.absUrl().split('?')[0] + '?post=' + id;
            }
        }

        function notifyCopied(e) {
            if (vm.post.id) {
                notifySrv.success('wall.successToClipboard');
            } else {
                notifySrv.error('errorCodeMessages.messageError');
            }

            e && e.preventDefault();
        }

        function unwatchPost() {
            if (vm.isActionsEnabled && vm.post.isWatched) {

                vm.isActionsEnabled = false;

                wallPostRepository.unwatchPost(vm.post).then(function() {
                    vm.isActionsEnabled = true;
                    vm.post.isWatched = false;
                }, vm.handleErrorMessage);
            }
        }

        function watchPost() {
            if (vm.isActionsEnabled && !vm.post.isWatched) {

                vm.isActionsEnabled = false;

                wallPostRepository.watchPost(vm.post).then(function() {
                    vm.isActionsEnabled = true;
                    vm.post.isWatched = true;
                }, vm.handleErrorMessage);
            }
        }
    }
}());
