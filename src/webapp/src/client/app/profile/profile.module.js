(function () {
    'use strict';
    window.modules = window.modules || [];
    window.modules.push('simoonaApp.Profile');

    angular
        .module('simoonaApp.Profile', [
            'ui.router',
            'ngLodash',
            'simoonaApp.Common'
        ])
        .config(route);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
        // Profile
            .state('Root.WithOrg.Client.Profiles', {
                abstract: true,
                url: '/Profiles',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Client.Profiles.List', {
                url: '',
                redirectTo: 'Root.WithOrg.Client.Profiles.Details'
            })
            .state('Root.WithOrg.Client.Profiles.Details', {
                url: '/:id',
                templateUrl: 'app/profile/profile-details/profile-details.html',
                controller: 'profilesDetailsController',
                resolve: {
                    model: [
                        '$stateParams', 'profileRepository', 'authService',
                        function ($stateParams, profileRepository, authService) {
                            if (!$stateParams.id) {
                                $stateParams.id = authService.identity.userId;
                            }

                            return profileRepository.getDetails($stateParams);
                        }
                    ]
                }
            })
            .state('Root.WithOrg.Client.Profiles.Edit', {
                url: '/:id/Edit/:tab',
                templateUrl: 'app/profile/profile-edit/profile-edit.html',
                controller: 'profileEditController',
                reloadOnSearch: true,
                resolve: {
                    tabName: [
                        '$stateParams',
                        function ($stateParams) {
                            return $stateParams.tab || 'personal';
                        }
                    ]
                }
            });
    }
})();
