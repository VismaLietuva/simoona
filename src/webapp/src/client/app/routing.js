(function() {
    'use strict';

    angular
        .module('simoonaApp')
        .config(route);

    route.$inject = [
        '$stateProvider',
        '$locationProvider',
        '$urlMatcherFactoryProvider'
    ];

    function route($stateProvider, $locationProvider, $urlMatcherFactoryProvider) {

        $locationProvider.html5Mode(true);

        $urlMatcherFactoryProvider.caseInsensitive(true);
        $urlMatcherFactoryProvider.strictMode(false);

        $stateProvider

        // RedirectTo state
            .state('RedirectTo', {
            url: '/redirectTo/:state/',
            controller: [
                '$stateParams', '$state',
                function($stateParams, $state) {
                    $state.go($stateParams.state, '', {
                        location: 'replace'
                    });
                }
            ]
        })

        .state('Root.WithoutOrg.Home', {
            url: '',
            templateUrl: 'app/auth/login/login.html',
            controller: 'loginController',
            controllerAs: 'vm'
        })

        .state('Root.WithOrg.Home', {
            url: '',
            templateUrl: 'app/auth/login/loginAuth.html',
            controller: 'loginAuthController',
            controllerAs: 'vm'
        })

        // Root
        .state('Root', {
            abstract: true,
            url: ''
        })

        // all states without organization id must be before state 'Root.WithOrg'
        .state('Root.WithoutOrg', {
                abstract: true,
                url: '',
                views: {
                    '': {
                        templateUrl: 'app/layout/layout.html',
                        controller: 'layoutController',
                        controllerAs: 'vm'
                    },
                    'left-menu@Root.WithoutOrg': {
                        templateUrl: 'app/layout/left-menu/left-menu.html',
                        controller: 'leftMenuController',
                        controllerAs: 'vm'
                    },
                    'navigation-bar@Root.WithoutOrg': {
                        templateUrl: 'app/layout/navigation-bar/navigation-bar.html',
                        controller: 'navigationBarController',
                        controllerAs: 'vm'
                    }
                }
            })
            .state('Root.WithOrg', {
                abstract: true,
                url: '/:organizationName',
                views: {
                    '': {
                        templateUrl: 'app/layout/layout.html',
                        controller: 'layoutController',
                        controllerAs: 'vm'
                    },
                    'left-menu@Root.WithOrg': {
                        templateUrl: 'app/layout/left-menu/left-menu.html',
                        controller: 'leftMenuController',
                        controllerAs: 'vm'
                    },
                    'navigation-bar@Root.WithOrg': {
                        templateUrl: 'app/layout/navigation-bar/navigation-bar.html',
                        controller: 'navigationBarController',
                        controllerAs: 'vm'
                    }
                }
            })
            .state('Root.WithOrg.Error', {
                url: '/Error/:errorCode',
                templateUrl: 'app/error.html'
            })
            .state('Root.WithOrg.AccessDenied', {
                url: '/AccessDenied',
                templateUrl: 'app/access-denied.html'
            })
            .state('Root.WithOrg.Client', {
                abstract: true,
                url: '',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Admin', {
                abstract: true,
                url: '/Admin',
                template: '<ui-view></ui-view>'
            });
    }
})();