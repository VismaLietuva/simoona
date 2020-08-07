(function() {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .factory('wallRepository', wallRepository);

    wallRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function wallRepository($resource, endPoint) {
        var wallUrl = endPoint + '/Wall/';
        var wallWidgetsUrl = endPoint + '/WallWidgets/';

        var service = {
            createWall: createWall,
            editWall: editWall,
            getWallDetails: getWallDetails,
            deleteWall: deleteWall,
            getWallMembers: getWallMembers,
            expelMemberFromWall: expelMemberFromWall,
            getWidgetsInfo: getWidgetsInfo,
        };
        return service;

        /////////

        function createWall(wall) {
            return $resource(wallUrl + 'Create', {}, {
                get: {
                    method: 'GET',
                    isArray: false,
                    responseType: 'text'
                }
            }).save(wall).$promise;
        }

        function editWall(wall) {
            return $resource(wallUrl + 'Edit', '', {
                put: {
                    method: 'PUT'
                }
            }).put(wall).$promise;
        }

        function getWallDetails(wallId) {
            return $resource(wallUrl + 'Details').get({ wallId: wallId }).$promise;
        }

        function deleteWall(wallId) {
            return $resource(wallUrl + 'Delete').delete({ wallId: wallId }).$promise;
        }

        function getWallMembers(wallId) {
            return $resource(wallUrl + 'Members').query({ wallId: wallId }).$promise;
        }

        function expelMemberFromWall(wallId, userId) {
            return $resource(wallUrl + 'follow', {
                wallId: wallId,
                attendeeId: userId
            }, { put: { method: 'PUT' } }).put().$promise;
        }

        function getWidgetsInfo(){
            return $resource(wallWidgetsUrl + 'Get')
                .get()
                .$promise;
        }

    }
})();
