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

        function togglePostLike(id) {
            return $resource(postUrl + 'Like', {id: id}, {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }

        function toggleCommentLike(id) {
            return $resource(commentUrl + 'Like', {id: id}, {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }
    }
}());
