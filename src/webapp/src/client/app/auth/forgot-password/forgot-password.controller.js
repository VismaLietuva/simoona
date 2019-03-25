(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .controller('forgotPasswordController', forgotPasswordController);

        forgotPasswordController.$inject = [
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

    function forgotPasswordController($rootScope, $location, $window, $timeout, authService, notifySrv, endPoint, errorHandler, localeSrv) {
        /* jshint validthis: true */
        var vm = this;
        
        vm.organizationName = authService.getOrganizationNameFromUrl() || '';
        $rootScope.pageTitle = 'account.forgotPassword';

        vm.email = '';

        vm.resetPassword = resetPassword;
        vm.validateFields = validateFields;
        vm.redirectToLogin = redirectToLogin;

        init()

        ///////

        function init() {
            authService.setOrganizationName(vm.organizationName);
        }

        function validateFields() {
            var valid = true;
            if (!vm.email.length) {
                notifySrv.error('errorCodeMessages.messageAllFieldsAreRequired');
                valid = false;
            }

            return valid;
        }

        function resetPassword() {
            if (!validateFields()) {
                return;
            }

            const params = {
                email: vm.email
            }

            authService.requestPasswordReset(params).then(function(response){
                notifySrv.success("applicationUser.checkYourEmail");
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