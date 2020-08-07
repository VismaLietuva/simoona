(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('wallCommentRepository', wallCommentRepository);

    wallCommentRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function wallCommentRepository($resource, endPoint) {
        var commentUrl = endPoint + '/Comment/';

        var service = {
            createComment: createComment,
            editComment: editComment,
            deleteComment: deleteComment
        };
        return service;

        /////////

        function createComment(comment) {
            return $resource(commentUrl + 'Create').save({
                postId: comment.postId,
                messageBody: comment.messageBody,
                pictureId: comment.pictureId,
                mentionedUserIds: comment.mentionedUserIds
            }).$promise;
        }

        function editComment(comment) {
            return $resource(commentUrl + 'Edit', '', {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: comment.id,
                messageBody: comment.messageBody,
                pictureId: comment.pictureId
            }).$promise;
        }

        function deleteComment(comment) {
            return $resource(commentUrl + 'Hide', '', {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: comment.id
            }).$promise;
        }
    }
})();
