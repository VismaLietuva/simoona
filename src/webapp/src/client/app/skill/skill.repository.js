(function () {
    'use strict';

    const simoonaApp = angular.module('simoonaApp.Skill');

    simoonaApp.factory('skillRepository', skillRepository);

    skillRepository.$inject = ['$resource', 'endPoint'];

    function skillRepository($resource, endPoint) {
        const skillUrl = endPoint + '/Skill/';

        return {
            getForAutoComplete: function (s) {
                return $resource(skillUrl + 'GetForAutoComplete').query(s).$promise;
            },

            post: function (model) {
                return $resource(skillUrl + 'Post').post(model).$promise;
            }
        }
    }


})();