(function () {
    'use strict';

    angular
        .module('simoonaApp.Settings', [
            
        ])
        .config(route);

    route.$inject = [
        '$stateProvider',
        '$urlRouterProvider'
    ];

    function route($stateProvider, $urlRouterProvider) {
        $stateProvider
            .state('Root.WithOrg.Client.Settings', {
                abstract: true,
                url: '/Settings',
                templateUrl: 'app/settings/settings.html',
                controller: 'settingsController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Settings.General', {
                url: '/General',
                templateUrl: 'app/settings/general/general.html',
                controller: 'settingsGeneralController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Settings.Notifications', {
                url: '/Notifications',
                templateUrl: 'app/settings/notifications/notifications.html',
                controller: 'settingsNotificationsController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Settings.Providers', {
                url: '/Providers',
                templateUrl: 'app/settings/providers/providers.html',
                controller: 'settingsProvidersController',
                controllerAs: 'vm'
            });
        $urlRouterProvider
            .when('/:organizationName/Settings', '/:organizationName/Settings/General')
            .otherwise(function ($injector, $location) {
                var authService = $injector.get('authService');
                authService.redirectToPageNotFound();
            });
    }
})();