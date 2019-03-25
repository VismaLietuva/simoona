(function () {
    'use strict';

    angular
        .module('simoonaApp.OrganizationalStructure')
        .directive('aceOrgOrphans', orgOrphans);

    orgOrphans.$inject = [];

    function orgOrphans() {
        var directive = {
            restrict: 'AE',
            templateUrl: 'app/organizational-structure/orphans/orphans.html',
            scope: {},
            link: linkFunc
        };

        return directive;

        function linkFunc(scope, el) {
             /*jshint validthis: true*/

        }
    }


})();
