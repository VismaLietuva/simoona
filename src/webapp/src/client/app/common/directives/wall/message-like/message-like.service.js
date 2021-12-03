(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('messageLikeRepository', messageLikeRepository);

    messageLikeRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function messageLikeRepository($resource, endPoint) {
        var postUrl = endPoint + '/Post/';
        var commentUrl = endPoint + '/Comment/';

        var service = {
            togglePostLike: togglePostLike,
            toggleCommentLike: toggleCommentLike
        };
        return service;

        //////////

        function togglePostLike(id, type) {
            return $resource(postUrl + 'Like', '', {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: id,
                type: type
            }).$promise;
        }

        function toggleCommentLike(id, type) {
            return $resource(commentUrl + 'Like', '',  {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: id,
                type: type
            }).$promise;
        }
    }
}());
