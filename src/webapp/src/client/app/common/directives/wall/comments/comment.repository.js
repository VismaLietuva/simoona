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
        var userUrl = endPoint + '/User/';
        var commentUrl = endPoint + '/Comment/';

        var service = {
            createComment: createComment,
            editComment: editComment,
            deleteComment: deleteComment,
            getUsersForAutoComplete: getUsersForAutoComplete
        };
        return service;

        /////////

        function createComment(comment) {
            return $resource(commentUrl + 'Create').save({
                postId: comment.postId,
                messageBody: comment.messageBody,
                images: comment.images,
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
                images: comment.images
            }).$promise;
        }

        function getUsersForAutoComplete(searchString) {
            return $resource(userUrl + 'GetUsersForAutocomplete').query({
                s: searchString
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
