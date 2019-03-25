(function () {
    'use strict';

    window.modules = window.modules || [];
    window.modules.push('simoonaApp.OrganizationalStructure');

    angular
        .module('simoonaApp.OrganizationalStructure', [
            'ui.router',
            'simoonaApp.Common',
            'simoonaApp.Layout.LeftMenu'
        ])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider', '$windowProvider'];

    function route($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }
        $stateProvider
            .state('Root.WithOrg.Client.OrganizationalStructure', {
                url: '/OrganizationalStructure',
                templateUrl: 'app/organizational-structure/organizational-structure.html',
                controller: 'organizationalStructureController',
                controllerAs: 'vm'
            });
    }

    init.$inject = [
        'menuNavigationFactory',
        'leftMenuGroups',
        '$window'
    ];

    function init(menuNavigationFactory, leftMenuGroups, $window) {
        if (!$window.isPremium) {
            return;
        }
        menuNavigationFactory.defineLeftMenuItem({
            permission: 'ORGANIZATIONALSTRUCTURE_BASIC',
            url: 'Root.WithOrg.Client.OrganizationalStructure',
            active: 'Root.WithOrg.Client.OrganizationalStructure',
            resource: 'navbar.organizationalStructure',
            order: 2,
            group: leftMenuGroups.company
        });
    }

})();
