(function () {
    'use strict';

    window.modules = window.modules || [];
    window.modules.push('simoonaApp.Committee');
    
    angular
        .module('simoonaApp.Committee', [
            'ngFitText',
            'ui.router',
            'simoonaApp.Common'
        ])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider', '$windowProvider'];

    function route($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }
        $stateProvider
        .state('Root.WithOrg.Client.Committees', {
            abstract: true,
            url: '/Committees',
            template: '<ui-view></ui-view>'
        })
        .state('Root.WithOrg.Client.Committees.List', {
            url: '/List',
            templateUrl: 'app/committee/committee-list.html',
            controller: 'CommitteeController',
            resolve: {
                committees: [
                    'committeeRepository', function (committeeRepository) {
                        return committeeRepository.getAll( { includeProperties: 'Members,Leads,Delegates', orderBy: 'Name' });
                    }
                ]
            }
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
            permission: 'COMMITTEES_BASIC',
            url: 'Root.WithOrg.Client.Committees.List',
            active: 'Root.WithOrg.Client.Committees.List',
            resource: 'navbar.committees',
            order: 6,
            group: leftMenuGroups.company
        });
    }
})();
