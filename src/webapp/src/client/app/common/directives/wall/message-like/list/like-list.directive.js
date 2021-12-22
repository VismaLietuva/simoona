(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceMessageLikeList', messageLikeList)
        .constant('likeTypes', [
            { emoji: '👍', type: 0, resource: "common.emojiLike" },
            { emoji: '❤️', type: 1, resource: "common.emojiLove" },
            { emoji: '🤣', type: 2, resource: "common.emojiLol" },
            { emoji: '😲', type: 3, resource: "common.emojiWow"},
            { emoji: '👏', type: 4, resource: "common.emojiCongrats" },
            { emoji: '😢', type: 5, resource: "common.emojiSad" },
            { emoji: '😾', type: 6, resource: "common.emojiGrumpyCat" }
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
                    if (!!likes[i]) {
                        if(scope.user === null && likes[i].userId === authService.identity.userId) {
                            scope.user = likes[i];
                        }

                        scope.positionedLikes.push(likes[i]);
                        addLikeType(likes[i].type, likes[i]);
                    }
                }

                // Sorting types to always keep the same order of emojis
                scope.likeTypes.sort((a, b) => a.type > b.type ? 1 : -1);
            }

            function addLikeType(index, like) {
                if(!scope.likeTypes.contains(likeTypes[index])) {
                    scope.likeTypes.push(likeTypes[index]);
                }

                scope.filteredLikesByType[index].push(like);
            }
        }
    }
}());
