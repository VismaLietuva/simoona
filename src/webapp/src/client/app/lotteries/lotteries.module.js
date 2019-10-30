(function() {
    'use strict';

    modules = [
        'ui.router',
        'simoonaApp.Common'
    ];

    angular
        .module('simoonaApp.Lotteries', modules)
        .constant('lotteryStatuses', {
            drafted: 1,
            started: 2,
            aborted: 3,
            ended: 4,
            refundStarted: 5,
            refundLogsCreated: 6
        })
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
                resolve: {
                    lottery: function () {
                        return {};
                    }
                },
                data: {
                    authorizeRole: 'Admin'
                }
            })
            .state('Root.WithOrg.Admin.Lotteries.Edit', {
                abstract: false,
                url: '/:lotteryId/Edit',
                templateUrl: 'app/lotteries/lottery-manage.html',
                controller: 'lotteryManageController',
                controllerAs: 'vm',
                resolve: {
                    lottery: [
                        '$stateParams', 'lotteryRepository', function ($stateParams, lotteryRepository) {
                            return lotteryRepository.getLottery($stateParams.lotteryId);
                        },
                    ]
                },
                data: {
                    authorizeRole: 'Admin'
                }
            })
            .state('Root.WithOrg.Admin.Lotteries.Refund', {
                abstract: false,
                url: '/:lotteryId/Refunding',
                templateUrl: 'app/lotteries/lottery-refund.html',
                controller: 'lotteryRefundController',
                controllerAs: 'vm',
                resolve: {
                    lottery: [
                        '$stateParams', 'lotteryRepository', function () {
                            return {};
                        },
                    ]
                },
                data: {
                    authorizeRole: 'Admin'
                }
            })
    }
})();
