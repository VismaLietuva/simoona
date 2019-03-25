(function () {
    'use strict';

    angular
        .module('simoonaApp.Impersonate',[])
        .controller('impersonateController', impersonateController);

    impersonateController.$inject = [
        '$uibModalInstance',
        'userRepository',
        'authService'
    ];

    function impersonateController($uibModalInstance, userRepository, authService) {
        /* jshint validthis: true */
        var vm = this;

        vm.cancel = cancel;
        vm.impersonateUser = impersonateUser;
        vm.searchUsersForAutocomplete = searchUsersForAutocomplete;

        //////

        function searchUsersForAutocomplete(search) {
            return userRepository.getForAutocomplete(search);
        }

        function impersonateUser(user) {
            authService.impersonate(user.userName).then(function (response) {
                authService.getUserInfo(response.access_token).then(function (userInfo) {
                    authService.setAuthenticationData(userInfo, response.access_token);

                    authService.redirectToHome();
                });
            });

            $uibModalInstance.close();
        }

        function cancel() {
            $uibModalInstance.close();
        }
    }
})();
