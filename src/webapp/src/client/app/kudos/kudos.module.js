(function () {
    'use strict';

    window.modules = window.modules || [];
    window.modules.push('simoonaApp.Kudos');

    angular
        .module('simoonaApp.Kudos', [
            'ngLodash',
            'ui.router',
            'toaster',
            'chart.js',
            'simoonaApp.Common'
        ])
        .constant('modalTypes', {
            submit: 1,
            send: 2
        })
        .config(route)
        .run(init);

    route.$inject = [
        '$stateProvider'
    ];

    function route($stateProvider) {

        $stateProvider
            .state('Root.WithOrg.Client.Kudos', {
                abstract: true,
                url: '/Kudos',
                controller: 'KudosController',
                controllerAs: 'vm',
                templateUrl: 'app/kudos/kudos.html'
            })
            .state('Root.WithOrg.Client.Kudos.KudosAchievementBoard', {
                url: '/KudosAchievementBoard',
                controller: 'KudosAchievementBoard',
                controllerAs: 'vm',
                templateUrl: 'app/kudos/achievement-board/achievement-board.html'
            })
            .state('Root.WithOrg.Client.Kudos.KudosUserInformation', {
                url: '/KudosUserInformation/:userId?',
                controller: 'KudosUserInformationController',
                controllerAs: 'vm',
                templateUrl: 'app/kudos/user-information/user-information.html'
            })
            .state('Root.WithOrg.Client.Kudos.KudosLogList', {
                url: '/KudosLogList/:userId?',
                controller: 'kudosLogListController',
                controllerAs: 'vm',
                templateUrl: 'app/kudos/log-list/log-list.html'
            });
    }

    init.$inject = [
        'menuNavigationFactory',
        'leftMenuGroups'
    ];

    function init(menuNavigationFactory, leftMenuGroups) {
        menuNavigationFactory.defineLeftMenuItem({
            permission: 'KUDOS_BASIC',
            url: 'Root.WithOrg.Client.Kudos.KudosAchievementBoard',
            active: 'Root.WithOrg.Client.Kudos',
            resource: 'navbar.kudos',
            order: 2,
            group: leftMenuGroups.activities
        });
    }
})();
