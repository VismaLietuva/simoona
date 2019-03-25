(function () {
    'use strict';
    window.modules = window.modules || [];
    window.modules.push('simoonaApp.Office');
    angular.module('simoonaApp.Office', ['ui.router', 'simoonaApp.Office.Floor', 'simoonaApp.Map', 'nsPopover'])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider', '$windowProvider'];

    function route($stateProvider, $windowProvider) {

        if ($windowProvider.$get().isPremium) {
            $stateProvider.state('Root.WithOrg.Client.Office', {
                url: '/Office?floorId&roomId&coords&user', // GYTIS: THIS IS WRONG AND HAS TO BE FIXED. For example: /Office/:floorId/:roomId/... should be in separate child states. See ticket ROOM-137
                reloadOnSearch: false,
                templateUrl: 'app/office/office.html',
                controller: 'officeController'
            })
        }

        $stateProvider
            .state('Root.WithOrg.Admin.Offices', {
                abstract: true,
                url: '/Offices',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Admin.Offices.List', {
                url: '?sort&dir&page&s',
                templateUrl: 'app/office/office-list.html',
                controller: 'officeListController',
                reloadOnSearch: false,
                resolve: {
                    offices: [
                        '$stateParams', 'officeRepository',
                        function ($stateParams, officeRepository) {
                            $stateParams.includeProperties = 'Floors,Floors.Rooms,Floors.Rooms.ApplicationUsers';
                            return officeRepository.getPaged($stateParams);
                        }
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'OFFICE_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Offices.Create', {
                url: '/Create',
                templateUrl: 'app/office/office-manage.html',
                controller: 'officeModifyController',
                resolve: {
                    office: function () {
                        return {};
                    }
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'OFFICE_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Offices.Edit', {
                url: '/:id/Edit',
                templateUrl: 'app/office/office-manage.html',
                controller: 'officeModifyController',
                reloadOnSearch: false,
                resolve: {
                    office: [
                        '$stateParams', 'officeRepository',
                        function ($stateParams, officeRepository) {
                            return officeRepository.getOffice($stateParams.id);
                        }
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'OFFICE_ADMINISTRATION'
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
            permission: 'MAP_BASIC',
            url: 'Root.WithOrg.Client.Office',
            active: 'Root.WithOrg.Client.Office',
            resource: 'navbar.officeMap',
            order: 1,
            group: leftMenuGroups.company
        });
    }
})();
