(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceMessageLikeList', messageLikeList)
        .constant('likeTypes', [
            { emoji: '👍', likeType: 0 },
            { emoji: '❤️', likeType: 1 },
            { emoji: '🤣', likeType: 2 },
            { emoji: '😲', likeType: 3 },
            { emoji: '👏', likeType: 4 },
            { emoji: '😢', likeType: 5 },
            { emoji: '😾', likeType: 6 }
        ]);

    messageLikeList.$inject = [
        '$timeout',
        'authService',
        'likeTypes'
    ];

    function messageLikeList($timeout, authService, likeTypes) {
        var directive = {
            restrict: 'E',
            templateUrl: 'app/common/directives/wall/message-like/list/like-list.html',
            scope: {
                messageObject: '=',
                likeType: '@'
            },
            link: linkFunc
        };

        return directive;

        function linkFunc(scope) {
            $timeout(function() {
                scope.$watchCollection('messageObject', function() {
                    makeList();
                });
            });

            function makeList() {
                var likes = scope.messageObject.likes;

                scope.user = null;

                scope.positionedLikes = [];
                scope.likeTypes = [];
                scope.filteredLikesByType = Array.from(Array(likeTypes.length), () => []);

                if (!likes.length) {
                    return;
                }

                for (var i = 0; i < likes.length; i++) {
                    if (!!likes[i] && likes[i].userId === authService.identity.userId) {
                        scope.user = likes[i];
                        scope.positionedLikes.push(likes[i]);

                        addLikeType(likes[i].likeType, likes[i]);
                    }
                }

                for (var i = 0; i < likes.length; i++) {
                    if (!!likes[i] && likes[i].userId !== authService.identity.userId) {
                        scope.positionedLikes.push(likes[i]);

                        addLikeType(likes[i].likeType, likes[i]);
                    }
                }
            }

            function addLikeType(index, like) {
                if(!scope.likeTypes.contains(likeTypes[index])) {
                    scope.likeTypes.push(likeTypes[index]);

                    // Add empty entry, because first one is hidden in popover
                    scope.filteredLikesByType[index].push({});
                }

                scope.filteredLikesByType[index].push(like);
            }
        }
    }
}());
