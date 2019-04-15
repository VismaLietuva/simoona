(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .controller('resetPasswordController', resetPasswordController);

        resetPasswordController.$inject = [
        '$rootScope',
        '$location',
        '$window',
        '$timeout',
        'authService',
        'notifySrv',
        'endPoint',
        'errorHandler',
        'localeSrv'
    ];

    function resetPasswordController($rootScope, $location, $window, $timeout, authService, notifySrv, endPoint, errorHandler, localeSrv) {
        /* jshint validthis: true */
        var vm = this;

        vm.organizationName = authService.getOrganizationNameFromUrl() || '';
        $rootScope.pageTitle = 'account.passwordReset';

        vm.password = '';
        vm.confirmPassword = '';

        vm.resetPassword = resetPassword;
        vm.validateFields = validateFields;
        vm.redirectToLogin = redirectToLogin;

        init();

        ///////

        function init() {
            authService.setOrganizationName(vm.organizationName);
        }

        function validateFields() {
            var valid = true;
            if (!vm.password.length ||
                !vm.confirmPassword.length) {
                notifySrv.error('errorCodeMessages.messageAllFieldsAreRequired');
                valid = false;

                return;
            }
            if (vm.password !== vm.confirmPassword) {
                notifySrv.error('applicationUser.passwordsDontMatch');
                valid = false;
            }
            if(!/\d/.test(vm.password)) {
                notifySrv.error('applicationUser.passwordErrorMustContainDigit');
                valid = false;
            }
            if (!/[A-Z]/.test(vm.password)) {
                notifySrv.error('applicationUser.passwordErrorMustContainUpperCase');              
                valid = false;
            }

            return valid;
        }

        function resetPassword() {
            if (!validateFields()) {
                return;
            }

            const params = {
                password: vm.password,
                confirmPassword: vm.confirmPassword,
                code: authService.getTokenFromUrl(),
                email: authService.getUserNameFromUrl()
            }

            authService.resetPassword(params).then(function(response){
                notifySrv.success("applicationUser.passwordChangeSuccessful");
                redirectToLogin();
            }).catch(function(error) {
                notifySrv.error("errorCodeMessages.messageError");
            });
        }

        function redirectToLogin() {
            authService.redirectToLogin();
        }
    }
})();