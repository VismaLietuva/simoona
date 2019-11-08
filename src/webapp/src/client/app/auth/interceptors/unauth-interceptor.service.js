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
            var organizationName = $location.path().split('/')[1];

            if (response.status === 401) {
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
                sessionStorage.setItem("redirectAfterFailedLogin", location.href); 
            }
            service.redirectIfResponseUnauthorized(rejection);
            return $q.reject(rejection);
        }
    }
})();
