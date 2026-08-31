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
        var loggingOut = false;

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
                config.headers.Organization = authData.organizationName || auth.getOrganizationName() || auth.getOrganizationNameFromUrl();
                if (!config.headers.Authorization) {
                    config.headers.Authorization = 'Bearer ' + authData.token;
                }
            }

            return config;
        }

        function response(response) {
            $injector.get('authService').noteSuccessfulResponse();
            return response || $q.when(response);
        }

        function responseError(response) {
            var state = $injector.get('$state');
            var auth = $injector.get('authService');

            if (response.status === 401) {
                // Response interceptors run in reverse registration order, so this
                // counts the 401 before unauthInterceptor reads the tally.
                var absorptionExhausted = auth.noteUnauthorizedResponse();

                if ((!auth.isStoredTokenValid() || absorptionExhausted) && !loggingOut) {
                    loggingOut = true;
                    auth.logOut();
                }
            } else if (response.status === 403) {
                state.go('Root.WithOrg.AccessDenied');
            } else if (response.status === 400 && response.statusText === 'Invalid organization') {
                state.go('Root.WithoutOrg.Home');
            }

            return $q.reject(response);
        }
    }
})();
