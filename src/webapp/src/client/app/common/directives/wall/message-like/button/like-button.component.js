(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceMessageLikeButton', {
            replace: true,
            templateUrl: 'app/common/directives/wall/message-like/button/like-button.html',
            bindings: {
                messageObject: '=',
                likeType: '@'
            },
            controller: wallLikeButtonController,
            controllerAs: 'vm'
        });

    wallLikeButtonController.$inject = [
        'messageLikeRepository',
        'lodash',
        'authService',
        'likeTypes'
    ];

    function wallLikeButtonController(messageLikeRepository, lodash, authService, likeTypes) {
        /*jshint validthis: true */
        var vm = this;

        vm.likePost = likePost;
        vm.unlikePost = unlikePost;
        vm.likeComment = likeComment;
        vm.unlikeComment = unlikeComment;
        vm.likeTypes = likeTypes;

        function likePost($likeType) {
            if (vm.messageObject.id) {
                addLikeToList($likeType);
                vm.messageObject.isLiked = true;

                messageLikeRepository.togglePostLike(vm.messageObject.id, $likeType);
            }
        }

        function unlikePost() {
            if (vm.messageObject.id) {
                removeLikeFromList();
                vm.messageObject.isLiked = false;

                messageLikeRepository.togglePostLike(vm.messageObject.id);
            }
        }

        function likeComment($likeType) {
            if (vm.messageObject.id) {
                addLikeToList($likeType);
                vm.messageObject.isLiked = true;

                messageLikeRepository.toggleCommentLike(vm.messageObject.id, $likeType);
            }
        }

        function unlikeComment() {
            if (vm.messageObject.id) {
                removeLikeFromList();
                vm.messageObject.isLiked = false;

                messageLikeRepository.toggleCommentLike(vm.messageObject.id);
            }
        }

        function addLikeToList($likeType) {
            if (!vm.messageObject.likes) {
                vm.messageObject.likes = [];
            }

            vm.messageObject.likes.push({
                type: $likeType,
                pictureId: '',
                userId: authService.identity.userId,
                fullName: authService.identity.fullName
            });
        }

        function removeLikeFromList() {
            lodash.remove(vm.messageObject.likes, function(like) {
                return like.userId === authService.identity.userId;
            });
        }
    }

}());
