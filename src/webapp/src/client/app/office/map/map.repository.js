(function () {
    'use strict';

    angular.module('simoonaApp.Map')
        .factory('mapRepository', mapRepository);
    
    mapRepository.$inject = ['$resource', '$q', 'endPoint'];

    function mapRepository($resource, $q, endPoint) {
        var url = endPoint + '/Map/';

        return {
            getDefault: function() {
                return $resource(url + 'GetDefault').get().$promise;
            },
            getByFloor: function(floorId) {
                return $resource(url + 'GetByFloor').get({ floorId: floorId }).$promise;
            },
            getByOffice: function(officeId) {
                return $resource(url + 'GetByOffice').get({ officeId: officeId }).$promise;
            },
            getByRoom: function(roomId) {
                return $resource(url + 'GetByRoom').get({ roomId: roomId }).$promise;
            },
            getByApplicationUser: function(userName) {
                return $resource(url + 'GetByApplicationUser').get({ userName: userName }).$promise;
            }
        };
    }
})();