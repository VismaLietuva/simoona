(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .controller('loginController', loginController);

    loginController.$inject = [
        '$rootScope',
        '$location',
        '$window',
        'authService',
        'notifySrv',
        'errorHandler'
    ];

    function loginController($rootScope, $location, $window, authService, notifySrv, errorHandler) {
        /* jshint validthis: true */
        var vm = this;
        var hashArray = authService.getHashArrayFromUrl();
        var access_token = hashArray['access_token'];

        vm.organizationName = authService.getOrganizationNameFromUrl() || '';
        vm.isLoggingIn = !!access_token || authService.identity.isAuthenticated;
        $rootScope.pageTitle = 'account.login';

        vm.continueLogin = continueLogin;

        init();

        ///////

        function init() {
            if (access_token) {
                authService.getUserInfo(access_token).then(function(response) {
                    if (response.hasRegistered) {
                        authService.setAuthenticationData(response, access_token);
                        authService.redirectToHome();
                    }
                    if (hashArray['error']) {
                        notifySrv.error('account.loginFailure');
                    }
                });
            }
        }

        function continueLogin(){
            if (!vm.organizationName.length || vm.organizationName === 'Login') {
                notifySrv.error('errorCodeMessages.messageOrganizationNotFound');
                return;
            }

            vm.isLoading = true;
            authService.setOrganizationName(vm.organizationName);

            try {
                authService.redirectToLogin();
            }
            catch(error) {
                vm.isLoading = false;
                errorHandler.handleErrorMessage(error);
            }
        }
    }
})();
