(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('wallPostRepository', wallPostRepository);

    wallPostRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function wallPostRepository($resource, endPoint) {
        var postUrl = endPoint + '/Post/';
        var wallUrl = endPoint + '/Wall/';
        var eventWallUrl = endPoint + '/EventWall/';

        var service = {
            getPosts: getPosts,
            getAllPosts: getAllPosts,
            getPost: getPost,
            createPost: createPost,
            editPost: editPost,
            deletePost: deletePost,
            searchWall: searchWall,
            watchPost: watchPost,
            unwatchPost: unwatchPost,

            getEventPosts: getEventPosts,
            getEventPost: getEventPost,
            createEventPost: createEventPost
        };
        return service;

        /////////

        function getPosts(params) {
            return $resource(wallUrl + 'Posts').get(params).$promise;
        }

        function getAllPosts(params) {
            return $resource(wallUrl + 'AllPosts').get(params).$promise;
        }

        function getPost(postId) {
            return $resource(postUrl + 'GetPost').get({
                postId: postId
            }).$promise;
        }

        function createPost(post, wallId) {
            return $resource(postUrl + 'Create').save({
                wallId: wallId,
                messageBody: post.messageBody,
                pictureId: post.pictureId
            }).$promise;
        }

        function editPost(post) {
            return $resource(postUrl + 'Edit', '', {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: post.id,
                messageBody: post.messageBody,
                pictureId: post.pictureId
            }).$promise;
        }

        function deletePost(post) {
            return $resource(postUrl + 'Hide', '', {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: post.id
            }).$promise;
        }

        function searchWall(params) {
            return $resource(wallUrl + 'Search').get(params).$promise;
        }

        function watchPost(post) {
            return $resource(postUrl + 'Watch', '', {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: post.id
            }).$promise;
        }

        function unwatchPost(post) {
            return $resource(postUrl + 'Unwatch', '', {
                put: {
                    method: 'PUT'
                }
            }).put({
                id: post.id
            }).$promise;
        }

        function getEventPosts(params) {
            return $resource(eventWallUrl + 'Posts').get(params).$promise;
        }

        function getEventPost(postId) {
            return $resource(postUrl + 'GetEventPost').get({
                postId: postId
            }).$promise;
        }

        function createEventPost(post, wallId) {
            return $resource(postUrl + 'CreateEventPost').save({
                wallId: wallId,
                messageBody: post.messageBody,
                pictureId: post.pictureId
            }).$promise;
        }
    }
})();
