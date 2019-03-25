(function () {
    'use strict';

    angular
        .module('simoonaApp.Project', [
            'ui.router',
            'simoonaApp.Common'
        ])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Client.Projects', {
                abstract: true,
                url: '/Projects',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Client.Projects.List', {
                url: '/List',
                templateUrl: 'app/project/list/list.html',
                controller: 'projectListController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Projects.ProjectContent', {
                url: '/Details/:id',
                templateUrl: 'app/project/content/content.html',
                controller: 'projectContentController',
                controllerAs: 'vm',
                params: {
                    postNotification: null
                }
            })
            .state('Root.WithOrg.Client.Projects.Create', {
                url: '/Create',
                templateUrl: 'app/project/create/create-edit.html',
                controller: 'projectCreateController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Projects.Edit', {
                url: '/Edit/:id',
                templateUrl: 'app/project/create/create-edit.html',
                controller: 'projectCreateController',
                controllerAs: 'vm'
            });
    }

    init.$inject = [
        'menuNavigationFactory',
        'leftMenuGroups'
    ];

    function init(menuNavigationFactory, leftMenuGroups) {
        menuNavigationFactory.defineLeftMenuItem({
            permission: 'PROJECT_BASIC',
            url: 'Root.WithOrg.Client.Projects.List',
            active: 'Root.WithOrg.Client.Projects',
            resource: 'navbar.project',
            order: 5,
            group: leftMenuGroups.company
        });
    }
})();
