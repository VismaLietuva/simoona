(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .controller('registerController', registerController);

        registerController.$inject = [
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

    function registerController($rootScope, $location, $window, $timeout, authService, notifySrv, endPoint, errorHandler, localeSrv) {
        /* jshint validthis: true */
        var vm = this;

        vm.organizationName = authService.getOrganizationNameFromUrl() || '';
        $rootScope.pageTitle = 'account.register';

        vm.firstName = '';
        vm.lastName = '';
        vm.email = '';
        vm.password = '';
        vm.repeatedPassword = '';

        vm.register = register;
        vm.validateFields = validateFields;
        vm.redirectToLogin = redirectToLogin;

        init();

        ///////

        function init() {
            authService.setOrganizationName(vm.organizationName);
        }

        function validateFields() {
            var valid = true;
            if (!vm.email.length || 
                !vm.password.length || 
                !vm.repeatedPassword.length ||
                !vm.firstName.length ||
                !vm.lastName.length) {
                notifySrv.error('errorCodeMessages.messageAllFieldsAreRequired');
                valid = false;
            }
            if (vm.password !== vm.repeatedPassword) {
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

        function register() {
            if (!validateFields()) {
                return;
            }

            const user = {
                firstName: vm.firstName,
                lastName: vm.lastName,
                userName: vm.email,
                email: vm.email,
                password: vm.password,
                confirmPassword: vm.repeatedPassword,
            };
            
            authService.register(user).then(function(response) {
                notifySrv.success("applicationUser.registrationSuccessful");
                notifySrv.success("applicationUser.checkYourEmail");
                redirectToLogin();
            }).catch(function(error) {
                if (error.status === 400) {
                    notifySrv.error('applicationUser.emailAlreadyExsists');
                }
            });
        }

        function redirectToLogin() {
            authService.redirectToLogin();
        }
    }
})();
