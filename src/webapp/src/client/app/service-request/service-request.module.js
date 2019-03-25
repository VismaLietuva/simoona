(function () {
    'use strict';

    window.modules = window.modules || [];
    window.modules.push('simoonaApp.ServiceRequest');
    
    angular
        .module('simoonaApp.ServiceRequest', [
            'ui.router',
            'simoonaApp.Common'
        ])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider', '$windowProvider'];

    function route($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }
        $stateProvider
            .state('Root.WithOrg.Client.ServiceRequests', {
                abstract: true,
                url: '/ServiceRequests',
                template: '<ui-view></ui-view>',
                data: {
                    authorizePermission: 'SERVICEREQUESTS_BASIC'
                }
            })
            .state('Root.WithOrg.Client.ServiceRequests.List', {
                url: '/List?Id',
                params: {
                    openNewRequest: false
                },
                templateUrl: 'app/service-request/service-request-main.html',
                controller: 'ServiceRequestController',
                resolve: {
                    pagedServiceRequestList: [
                        'serviceRequestRepository', function (serviceRequestRepository) {
                            var filter = {
                                includeProperties: 'Priority, Status, Employee',
                                page: 1,
                                sortOrder: 'desc',
                                sortBy: 'Created',
                                search: '',
                                priority: '',
                                status: '',
                                serviceRequestCategory: ''
                            };
                            return serviceRequestRepository.getPaged(filter);
                        }
                    ]
                }
            });
    }

    init.$inject = [
        'menuNavigationFactory',
        'leftMenuGroups',
        '$window'
    ];

    function init(menuNavigationFactory, leftMenuGroups, $window) {
        if (!$window.isPremium) {
            return;
        }
        menuNavigationFactory.defineLeftMenuItem({
            permission: 'SERVICEREQUESTS_BASIC',
            url: 'Root.WithOrg.Client.ServiceRequests.List',
            active: 'Root.WithOrg.Client.ServiceRequests.List',
            resource: 'navbar.serviceRequest',
            order: 3,
            group: leftMenuGroups.activities
        });
    }
})();
