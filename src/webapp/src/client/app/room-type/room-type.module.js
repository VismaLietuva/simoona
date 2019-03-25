(function () {
    'use strict';

    angular.module('simoonaApp.RoomType', ['ui.router'])
        .config(route);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
            // RoomType
            .state('Root.WithOrg.Admin.RoomTypes', {
                abstract: true,
                url: '/RoomTypes',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Admin.RoomTypes.List', {
                url: '?sort&dir&page&s',
                templateUrl: 'app/room-type/room-type-list.html',
                controller: 'roomTypeController',
                reloadOnSearch: false,
                resolve: {
                    model: [
                        '$stateParams', 'roomTypeRepository', function($stateParams, roomTypeRepository) {
                            return roomTypeRepository.getPaged($stateParams);
                        }
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROOMTYPE_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.RoomTypes.Create', {
                url: '/Create',
                templateUrl: 'app/room-type/room-type-manage.html',
                controller: 'roomTypeManagerController',
                reloadOnSearch: false,
                resolve: {
                    roomType: function() {
                        return { name: '', color: '#FFFFFF' };
                    }
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROOMTYPE_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.RoomTypes.Edit', {
                url: '/:id/Edit',
                templateUrl: 'app/room-type/room-type-manage.html',
                controller: 'roomTypeManagerController',
                reloadOnSearch: false,
                resolve: {
                    roomType: [
                        '$stateParams', 'roomTypeRepository', function($stateParams, roomTypeRepository) {
                            $stateParams.includeProperties = 'Rooms';
                            return roomTypeRepository.get($stateParams);
                        }
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROOMTYPE_ADMINISTRATION'
                }
            });
    }
})();
