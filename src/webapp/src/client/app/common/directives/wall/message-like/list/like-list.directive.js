(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceMessageLikeList', messageLikeList);

    messageLikeList.$inject = [
        '$timeout',
        'authService'
    ];

    function messageLikeList($timeout, authService) {
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
                
                if (likes.length) {
                    for (var i = 0; i < likes.length; i++) {
                        if (!!likes[i] && likes[i].userId === authService.identity.userId) {
                            scope.user = likes[i];
                            scope.positionedLikes.push(likes[i]);
                        }
                    }

                    for (var j = 0; j < likes.length; j++) {
                        if (!!likes[j] && likes[j].userId !== authService.identity.userId) {
                            scope.positionedLikes.push(likes[j]);
                        }
                    }
                }
            }
        }

    }
}());
