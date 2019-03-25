'use strict';

(function() {
    angular.module('simoonaApp.Office.Floor.Room', []);
    var module = angular.module('simoonaApp.Office.Floor.Room');

    route.$inject = ['$stateProvider', '$windowProvider'];

    function route($stateProvider, $windowProvider) {
        if(!$windowProvider.$get().isPremium){
            return;
        }
        $stateProvider
            // Rooms
            .state('Root.WithOrg.Admin.Offices.Floors.Rooms', {
                abstract: true,
                url: '/:floorId/Rooms',
                template: '<ui-view></ui-view>',
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROOM_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Offices.Floors.Rooms.List', {
                url: '?sort&dir&page&s',
                templateUrl: 'app/office/floor/room/room-list.html',
                controller: 'roomController',
                reloadOnSearch: true,
                resolve: {
                    rooms: [
                        '$stateParams', 'roomRepository', function ($stateParams, roomRepository) {
                            $stateParams.includeProperties = "ApplicationUsers,RoomType,Floor,Floor.Office";
                           if ($stateParams.sort === null) {
                               $stateParams.sort = 'Name';
                           }
                            return roomRepository.getPaged($stateParams);
                        }
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROOM_ADMINISTRATION'
                }
            });
    }

    module.config(route);
    
})();
