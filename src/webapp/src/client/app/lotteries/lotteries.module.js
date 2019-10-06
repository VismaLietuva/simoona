(function() {
    'use strict';

    modules = [
        'ui.router',
        'simoonaApp.Common'
    ];

    angular
        .module('simoonaApp.Lotteries', modules)
        .config(config);

    config.$inject = ['$stateProvider'];

    function config($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.Lotteries', {
                abstract: true,
                url: '/Lotteries',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Admin.Lotteries.List', {
                abstract: false,
                url: '/List',
                templateUrl: 'app/lotteries/lottery-list.html',
                controller: 'lotteryListController',
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
            })
            .state('Root.WithOrg.Admin.Lotteries.Create', {
                abstract: false,
                url: '/Create',
                templateUrl: 'app/lotteries/lottery-manage.html',
                controller: 'lotteryManageController',
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
