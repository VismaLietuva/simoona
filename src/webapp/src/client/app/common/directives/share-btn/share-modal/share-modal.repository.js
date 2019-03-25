(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('shareModalRepository', shareModalRepository);

    shareModalRepository.$inject = ['$resource', 'endPoint'];

    function shareModalRepository($resource, endPoint) {
        var shareRequestUrl = endPoint;

        var service = {
            shareOnWall: shareOnWall
        };

        return service;

        ////////////////
        
        function shareOnWall(sharedItem, sharedItemCategory) {
            return $resource(shareRequestUrl + '/' + sharedItemCategory + '/Share').save(sharedItem).$promise;
        }

    }
})();