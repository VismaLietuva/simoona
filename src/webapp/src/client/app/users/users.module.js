(function() {
    'use strict';

    angular
        .module('simoonaApp.Users', ['ui.router'])
        .config(config);

    config.$inject = ['$stateProvider'];

    function config($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.ApplicationUsers', {
                url: '/Users?roomId&sort&dir&page&s&filter',
                templateUrl: 'app/users/users.html',
                controller: 'applicationUserListController',
                controllerAs: 'vm',
                resolve: {
                    usersModel: [
                        '$stateParams', 'userRepository',
                        function($stateParams, userRepository) {
                            return userRepository.getUsersListPaged($stateParams);
                        }
                    ]
                },
                reloadOnSearch: false,
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'APPLICATIONUSER_ADMINISTRATION'
                }
            });
    }
})();
