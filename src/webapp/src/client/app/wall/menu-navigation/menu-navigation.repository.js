(function() {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .factory('wallMenuNavigationRepository', wallMenuNavigationRepository);

    wallMenuNavigationRepository.$inject = ['$resource', 'endPoint'];

    function wallMenuNavigationRepository($resource, endPoint) {
        var url = endPoint + '/Wall/';

        var service = {
            listWalls: listWalls,
            toggleFollowWall: toggleFollowWall
        };
        return service;

        /////

        function listWalls(wallFilter) {
            return $resource(url + 'list').query({filter: wallFilter}).$promise;
        }

        function toggleFollowWall(wallId) {
            return $resource(url + 'follow', {
                wallId: wallId
            }, { put: { method: 'PUT' } }).put().$promise;
        }
    }
}());
