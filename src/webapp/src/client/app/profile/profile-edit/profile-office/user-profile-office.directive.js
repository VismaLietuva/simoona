(function () {
    'use strict';

    angular.module('simoonaApp.Profile')
        .directive('profileOffice', profileOffice);

    profileOffice.$inject = [];

    function profileOffice() {

        return {
            templateUrl: 'app/profile/profile-edit/profile-office/user-profile-office.html',
            restrict: 'AE',
            scope: {
                model: '='
            },
            controller: controller
        };
    }

    controller.$inject = ['$scope', 'authService', 'notifySrv', 'userRepository',
                          'officeRepository', 'floorRepository', 'roomRepository', 'profileRepository'];

    function controller($scope, authService, notifySrv, userRepository,
        officeRepository, floorRepository, roomRepository, profileRepository) {

        $scope.loadOffices = loadOffices;
        $scope.loadFloors = loadFloors;
        $scope.loadRooms = loadRooms;
        $scope.saveInfo = saveInfo;

        var loadRoom = function (room) {
            loadOffices();
            loadFloors(room.officeId);
            loadRooms(room.floorId);
        };

        init();

        function init() {
            $scope.errors = [];
            $scope.info = $scope.model;
            $scope.room = {};
            $scope.userRoom = {
                id: $scope.info.roomId,
                floorId: $scope.info.room ? $scope.info.room.floorId : null,
                officeId: $scope.info.room ? $scope.info.room.floor ? $scope.info.room.floor.officeId : null : null
            };

            $scope.isAdmin = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);
            $scope.isNotNewUser = authService.identity.isAuthenticated && !(authService.isInRole('NewUser'));

            var room = $scope.info.roomToConfirm || $scope.info.room;
            if (room) {
                $scope.room.id = room.id;
                $scope.room.floorId = room.floorId;
                $scope.room.officeId = room.floor ? room.floor.officeId : null;
            } else {
                $scope.room.id = null;
                $scope.room.floorId = null;
                $scope.room.officeId = null;
            }
            loadRoom($scope.room);
        }

        function onErrorResponse(response) {
            $scope.errors = response.data;
        }

        // Load cascading dropdown
        function loadOffices() {
            officeRepository.getAll().then(function (offices) {
                    $scope.offices = offices;
                },
                function (response) {
                    $scope.errors = response.data;
                });
        }

        function loadFloors(officeId) {
            if (officeId) {
                return floorRepository.getByOffice(officeId).then(function (floors) {
                    $scope.floors = floors;
                });
            } else {
                $scope.floors = null;
                $scope.rooms = null;
            }

            $scope.room.floorId = null;
            $scope.room.id = null;
        }

        function loadRooms(floorId) {
            if (floorId) {
                return roomRepository.getByFloor(floorId, void 0, true, false).then(function (rooms) {
                    $scope.rooms = rooms;
                });
            } else {
                $scope.rooms = null;
            }
            $scope.room.id = null;
        }

        function saveInfo(room, userId) {
            $scope.errors = [];
            profileRepository.putOfficeInfo({
                id: userId,
                roomId: room.id
            }).then(function () {
                notifySrv.success('common.infoSaved');
            }, onErrorResponse);
        }
    }
})();
