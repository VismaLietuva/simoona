(function() {
    'use strict';

    modules = [
        'ui.router',
        'simoonaApp.Common',
        'simoonaApp.Customization.ExternalLinks',
        'simoonaApp.Customization.KudosTypes',
        'simoonaApp.Customization.OrganizationSettings',
        'simoonaApp.Customization.JobTitles',
        'simoonaApp.Customization.ServiceRequestsTypes',
        'simoonaApp.Customization.KudosShop', 
        'simoonaApp.Customization.EventTypes'
    ];

    angular
        .module('simoonaApp.Customization', modules)
        .config(config);

    config.$inject = ['$stateProvider'];

    function config($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.Customization', {
                abstract: true,
                url: '/Customization',
                template: '<ui-view></ui-view>',
                data: {
                    authorizeRole: 'Admin',
                    authorizeOneOfPermissions: [
                        'EVENT_ADMINISTRATION',
                        'SERVICEREQUESTS_ADMINISTRATION',
                        'ORGANIZATION_ADMINISTRATION',
                        'JOB_ADMINISTRATION',
                        'KUDOSSHOP_ADMINISTRATION',
                        'KUDOS_ADMINISTRATION',
                        'EXTERNALLINK_ADMINISTRATION'
                    ]
                }
            })
            .state('Root.WithOrg.Admin.Customization.List', {
                url: '',
                templateUrl: 'app/customization/customization.html',
                controller: 'customizationController',
                controllerAs: 'vm',
                data: {
                    authorizeRole: 'Admin',
                    authorizeOneOfPermissions: [
                        'EVENT_ADMINISTRATION',
                        'SERVICEREQUESTS_ADMINISTRATION',
                        'ORGANIZATION_ADMINISTRATION',
                        'JOB_ADMINISTRATION',
                        'KUDOSSHOP_ADMINISTRATION',
                        'KUDOS_ADMINISTRATION',
                        'EXTERNALLINK_ADMINISTRATION'
                    ]
                }
            });
    }
})();
