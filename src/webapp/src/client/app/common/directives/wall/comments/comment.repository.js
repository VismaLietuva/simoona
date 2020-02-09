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
        var wallUrl = endPoint + '/Wall/';

        var service = {
            createComment: createComment,
            editComment: editComment,
            deleteComment: deleteComment,
            createEventComment: createEventComment
        };
        return service;

        /////////

        function createComment(comment) {
            return $resource(commentUrl + 'Create').save({
                postId: comment.postId,
                messageBody: comment.messageBody,
                pictureId: comment.pictureId
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

        function createEventComment(comment) {
            return $resource(commentUrl + 'CreateEventComment').save({
                postId: comment.postId,
                messageBody: comment.messageBody,
                pictureId: comment.pictureId
            }).$promise;
        }
    }
})();
