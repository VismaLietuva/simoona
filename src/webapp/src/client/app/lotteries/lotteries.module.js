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
                abstract: false,
                url: '/Lotteries',
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
            });
    }
})();
