(function () {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .factory('authInterceptor', authInterceptor);

    authInterceptor.$inject = [
        '$q',
        '$injector',
        'localStorageService'
    ];

    function authInterceptor($q, $injector, localStorageService) {
        var service = {
            request: request,
            response: response,
            responseError: responseError
        };
        return service;

        //////

        function request(config) {
            var auth = $injector.get('authService');
            var authData = localStorageService.get('authorizationData');

            config.headers = config.headers || {};
            config.headers.Organization = auth.getOrganizationName() || auth.getOrganizationNameFromUrl();

            if (!!authData && !!authData.token) {
                config.headers.Organization = authData.organizationName;
                if (!config.headers.Authorization) {
                    config.headers.Authorization = 'Bearer ' + authData.token;
                }
            }

            return config;
        }

        function response(response) {
            return response || $q.when(response);
        }

        function responseError(response) {
            var state = $injector.get('$state');
            var auth = $injector.get('authService');

            if (response.status === 401) {
                auth.logOut();
            } else if (response.status === 403) {
                state.go('Root.WithOrg.AccessDenied');
            } else if (response.status === 400 && response.statusText === 'Invalid organization') {
                state.go('Root.WithoutOrg.Home');
            }

            return $q.reject(response);
        }
    }
})();
