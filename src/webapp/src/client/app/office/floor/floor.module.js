'use strict';

(function() {
    angular.module('simoonaApp.Office.Floor', ['simoonaApp.Office.Floor.Room']);
    var module = angular.module('simoonaApp.Office.Floor');
    module.config(['$stateProvider', function ($stateProvider) {
        $stateProvider
            // Floors
            .state('Root.WithOrg.Admin.Offices.Floors', {
                abstract: true,
                url: '/:officeId/Floors',
                template: '<ui-view></ui-view>',
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'FLOOR_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Offices.Floors.List', {
                url: '?sort&dir&page&s',
                templateUrl: 'app/office/floor/floor-list.html',
                controller: 'floorController',
                resolve: {
                    model: [
                        '$stateParams', 'floorRepository', function ($stateParams, floorRepository) {
                            return floorRepository.getPaged($stateParams);
                        }
                    ]
                },
                reloadOnSearch: true,
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'FLOOR_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Offices.Floors.Create', {
                url: '/Create',
                templateUrl: 'app/office/floor/floor-manage.html',
                controller: 'floorManageController',
                resolve: {
                    model: function() {
                        return {};
                    }
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'FLOOR_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Offices.Floors.Edit', {
                url: '/:id/Edit',
                templateUrl: 'app/office/floor/floor-manage.html',
                controller: 'floorManageController',
                resolve: {
                    model: [
                        '$stateParams', 'floorRepository', function($stateParams, floorRepository) {
                            $stateParams.includeProperties = 'Picture,Office';
                            return floorRepository.get($stateParams);
                        }
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'FLOOR_ADMINISTRATION'
                }
            });
    }]);
})();
