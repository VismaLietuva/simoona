(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth', ['ui.router'])
        .constant('roles', {
            admin: 'Admin',
            external: 'External'
        })
        .run(execute)
        .config(config);

    execute.$inject = [
        '$rootScope',
        '$state',
        '$window',
        'authService',
        'appConfig',
        'roles'
    ];

    function execute($rootScope, $state, $window, authService, appConfig, roles) {
        $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {

            if (toState.redirectTo) {
                event.preventDefault();
                $state.go(toState.redirectTo, toParams, { location: 'replace' });
            }

            if (location.protocol === 'http:' && location.host === 'app.simoona.com') {
                $window.location.href = 'https://' + location.host + '/' + toParams.organizationName;
            }

            if (authService.identity.isAuthenticated 
                && authService.isInRole(roles.external) 
                && (!toState.name.contains('Root.WithOrg.Client.Events') && !toState.name.contains('Root.WithOrg.LogOff'))) {
                event.preventDefault();
                authService.redirectToEvents();
            }

            if (authService.identity.roles.contains('NewUser') && !appConfig.allowedStatesForNewUser.contains(toState.name)) {
                event.preventDefault();
                authService.redirectToHome();
            }

            if (authService.identity.isAuthenticated && toState.name.contains('Root.WithoutOrg') && authService.identity.organizationName) {
                event.preventDefault();
                authService.redirectToHome();
            }

            if (toState.data && toState.data.permissions && !authService.hasPermissions(toState.data.permissions)) {
                event.preventDefault();
                authService.redirectToLogin();
            }

            if (toState.name.contains('Root.WithOrg') && authService.identity.organizationName && toParams.organizationName !== authService.identity.organizationName) {
                event.preventDefault();
                authService.redirectToHome();
            }

            if (toState.name.contains('Root.WithOrg.Login') && authService.identity.isAuthenticated && authService.identity.organizationName && toParams.organizationName === authService.identity.organizationName) {
                event.preventDefault();
                authService.redirectToHome();
            }

            if (toState.data && toState.data.authorizePermission && !authService.hasPermissions([toState.data.authorizePermission])) {
                event.preventDefault();
                authService.redirectToAccessDenied();
            }

            if (toState.data && toState.data.authorizeOneOfPermissions && !authService.hasOneOfPermissions(toState.data.authorizeOneOfPermissions)) {
                event.preventDefault();
                authService.redirectToAccessDenied();
            }
        });
    }

    config.$inject = ['$stateProvider', '$httpProvider'];

    function config($stateProvider, $httpProvider) {
        $stateProvider.state('Root.WithOrg.Login', {
            url: '/Login',
            templateUrl: 'app/auth/login/loginAuth.html',
            controller: 'loginAuthController',
            controllerAs: 'vm'
        })
        .state('Root.WithOrg.LogOff', {
            url: '/LogOff',
            controller: 'logOffController'
        })
        .state('Root.WithOrg.Register', {
            url: '/Register',
            templateUrl: 'app/auth/register/register.html',
            controller: 'registerController',
            controllerAs: 'vm'
        })
        .state('Root.WithOrg.Forgot', {
            url: '/Forgot',
            templateUrl: 'app/auth/forgot-password/forgot-password.html',
            controller: 'forgotPasswordController',
            controllerAs: 'vm'
        })
        .state('Root.WithOrg.Reset', {
            url: '/Reset/*userName/Token/*token',
            templateUrl: 'app/auth/reset-password/reset-password.html',
            controller: 'resetPasswordController',
            controllerAs: 'vm'
        })
        .state('Root.WithOrg.Verify', {
            url: '/Verify/*userName/Token/*token',
            controller: 'verifyEmailController',
            controllerAs: 'vm'
        })
        .state('Root.WithoutOrg.Login', {
            url: '/Login',
            templateUrl: 'app/auth/login/login.html',
            controller: 'loginController',
            controllerAs: 'vm'
        })

        $httpProvider.interceptors.push('unauthInterceptor');
        $httpProvider.interceptors.push('authInterceptor');
    }
})();
