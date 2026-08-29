(function () {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .factory('unauthInterceptor', unauthInterceptor);

    unauthInterceptor.$inject = [
        '$q',
        '$injector',
        '$location'
    ];

    function unauthInterceptor($q, $injector, $location) {
        var service = {
            redirectIfResponseUnauthorized: redirectIfResponseUnauthorized,
            response: response,
            responseError: responseError
        };
        return service;

        ///////

        function redirectIfResponseUnauthorized(response) {
            var state = $injector.get('$state');
            var auth = $injector.get('authService');
            var organizationName = $location.path().split('/')[1];

            if (response.status === 401) {
                // Same guard as authInterceptor: don't redirect to Login if the
                // client-side token is still valid. This absorbs transient server-side
                // 401s (clock skew, tenant race) instead of ejecting the user - but
                // only for a few in a row, so a token the server rejects permanently
                // sends the user to Login instead of leaving the page spinning.
                if (auth.isStoredTokenValid() && !auth.hasExhaustedUnauthorizedAbsorption()) {
                    return;
                }
                state.go('Root.WithOrg.Login', {
                    organizationName: organizationName
                }, {
                    reload: true
                });
            }
        }

        function response(response) {
            service.redirectIfResponseUnauthorized(response);
            return response || $q.when(response);
        }

        function responseError(rejection) {
            if(!location.href.contains("/Login"))
            {
                    if(sessionStorage.getItem("redirectAfterFailedLogin") === null)
                    {
                        sessionStorage.setItem("redirectAfterFailedLogin", location.href); 
                    }
            }
            service.redirectIfResponseUnauthorized(rejection);
            return $q.reject(rejection);
        }
    }
})();
