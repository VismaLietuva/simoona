(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .controller('verifyEmailController', verifyEmailController);

        verifyEmailController.$inject = [
        '$rootScope',
        '$location',
        '$window',
        'authService',
        'notifySrv',
        'errorHandler'
    ];

    function verifyEmailController($rootScope, $location, $window, authService, notifySrv, errorHandler) {
        /* jshint validthis: true */
        var vm = this;

        vm.organizationName = authService.getOrganizationNameFromUrl() || '';
        $rootScope.pageTitle = 'account.login';

        init();

        ///////

        function init() {
            authService.setOrganizationName(vm.organizationName);

            const params = {
                code: authService.getTokenFromUrl(),
                email: authService.getUserNameFromUrl('/Verify/')
            }

            authService.verifyEmail(params).then(function (res) {
                notifySrv.success('applicationUser.emailVerifiedSucceessfully');
                authService.redirectToLogin();
            }).catch(function (error) {
                notifySrv.error('applicationUser.emailVerifiedUnsucceessfully');
                authService.redirectToLogin();
            });
        }
    }
})();
