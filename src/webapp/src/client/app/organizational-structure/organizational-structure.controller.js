(function () {
    'use strict';

    angular
        .module('simoonaApp.OrganizationalStructure')
        .controller('organizationalStructureController', organizationalStructureController);

    organizationalStructureController.$inject = [
        '$rootScope',
        '$translate'
    ];

    function organizationalStructureController($rootScope, $translate) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'organizationalStructure.organizationalStructureTitle';

        vm.resetOrgTree = resetOrgTree;

        /////////////

        function resetOrgTree(resetTreeFn) {
            vm.resetTreeFn = resetTreeFn;
        }
    }
})();
