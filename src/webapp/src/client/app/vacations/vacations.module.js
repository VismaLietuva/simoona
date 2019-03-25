(function () {
    'use strict';
    window.modules = window.modules || [];
    window.modules.push('simoonaApp.Vacations');
    angular
        .module('simoonaApp.Vacations', [
            'ui.router',
            'simoonaApp.Common'
        ])
        .config(route)
        .run(init);

    route.$inject = [
        '$stateProvider',
        '$windowProvider'
    ];

    function route($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }

        $stateProvider
            .state('Root.WithOrg.Client.Vacations', {
                abstract: true,
                url: '/Vacation',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Client.Vacations.List', {
                url: '/List',
                controller: 'vacationsController',
                controllerAs: 'vm',
                templateUrl: 'app/vacations/vacations-list.html'
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
            permission: 'VACATIONS_BASIC',
            url: 'Root.WithOrg.Client.Vacations.List',
            active: 'Root.WithOrg.Client.Vacations',
            resource: 'navbar.vacation',
            order: 5,
            group: leftMenuGroups.activities
        });
    }
})();
