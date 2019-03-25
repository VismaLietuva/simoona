(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .factory('qualificationLevelRepository', qualificationLevelRepository);

    qualificationLevelRepository.$inject = ['$resource', 'genericSrv', 'endPoint'];

    function qualificationLevelRepository($resource, genericSrv, endPoint) {
        var url = endPoint + '/QualificationLevel/';

        var qualificationLevelRepository = new genericSrv(url);

        return qualificationLevelRepository;
    }
})();