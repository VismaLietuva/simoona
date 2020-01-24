(function () {
    'use strict';

    angular.module('simoonaApp.Office')
        .controller('officeController', officeController);

    officeController.$inject = [
        '$scope',
        '$rootScope',
        '$state',
        '$location',
        'mapRepository',
        '$advancedLocation',
        'roomRepository',
        'userRepository',
        'authService',
        '$translate'
    ];

    function officeController($scope, $rootScope, $state, $location, mapRepository, $advancedLocation, roomRepository, userRepository, authService) {

        $scope.resetStateParams = resetStateParams;
        $scope.setScopeParams = setScopeParams;
        $scope.initialize = initialize;
        $scope.loadData = loadData;

        $scope.mapControl = {};
        $scope.params = {};

        $rootScope.pageTitle = 'map.nameMap';

        if (!authService.identity.isAuthenticated) {
            return;
        }

        activate();

        function activate() {


            $rootScope.$on('floorChange', function (e, floor) {
                resetStateParams();
                setScopeParams();
                $scope.params.floorId = floor.id;
                $scope.loadData();
            });

            $scope.$on('roomClicked', function (e, room) {
                var tempFloorId = $scope.params.floorId;
                resetStateParams();
                setScopeParams();
                $scope.params.floorId = tempFloorId;

                $location.search({ roomId: room.id });
                $scope.$apply();
            });

            $scope.$watch(function () {
                return $location.search().roomId;
            }, function (newValue, oldValue) {
                if (newValue == oldValue)
                    return;

                if (!$location.search().roomId) {
                    $scope.params.room = null;
                    $scope.params.roomId = null;
                    $scope.mapControl.deselectRoom();
                    return;
                }
                $scope.params.roomId = $location.search().roomId;
                roomRepository.get({ id: $location.search().roomId, includeProperties: 'Floor,RoomType' }).then(function (room) {
                    $scope.params.floorId = room.floorId;
                    $scope.params.room = room;
                });
            }, true);

            $scope.$watch(function () {
                return $location.search().user;
            }, function (newValue, oldValue) {
                if (newValue == oldValue)
                    return;

                if (!$location.search().user) {
                    $scope.params.applicationUser = null;
                    $scope.params.user = null;
                    if (!$location.search().roomId) {
                        $scope.params.room = null;
                        $scope.params.roomId = null;
                        $scope.mapControl.deselectRoom();
                    }
                    return;
                }
            }, true);

            $scope.$on('roomUnselected', function (e) {
                var tempFloorId = $scope.params.floorId;
                resetStateParams();
                setScopeParams();
                $scope.params.floorId = tempFloorId;
                $scope.$apply();
            });

            $scope.$on('usersBarItemHiglighted', function (e, roomId) {
                var rooms = $.grep($scope.floor.rooms, function (e) { return e.id === roomId; });

                if (rooms.length > 0) {
                    $scope.params.room = rooms[0];
                }

                $scope.params.highlightedRoomId = roomId;
            });

            $scope.initialize();
        }

        function loadData() {
            if ($scope.params.floorId === 'default') {
                mapRepository.getDefault().then(function (model) {
                    populateScopeWithModel(model);
                    $scope.params.floorId = model.floor ? model.floor.id: null;
                    prepareForMap(model.floor);
                });
            } else {
                mapRepository.getByFloor($scope.params.floorId).then(function (model) {
                    populateScopeWithModel(model);
                    $scope.params.floorId = model.floor ? model.floor.id : null;
                    prepareForMap(model.floor);
                });
            }
        }

        function populateScopeWithModel(model) {
            $scope.allOffices = model.allOffices;
            $scope.floor = model.floor;
            $scope.office = model.office;
            $scope.roomTypes = model.floor ? model.floor.roomTypes : null;
        }

        function setScopeParams() {
            $scope.params.roomId = $state.params.roomId;
            $scope.params.user = $state.params.user;
            $scope.params.coords = $state.params.coords;

            $scope.params.floorId = null;
            $scope.params.room = null;
            $scope.params.applicationUser = null;
        }

        function resetStateParams() {
            delete $state.params.roomId;
            delete $state.params.user;
            delete $state.params.coords;

            $location.search({});
        }

        function initialize() {
            $scope.setScopeParams();

            if ($scope.params.user) {
                userRepository.getByUserName($location.search().user, 'Room').then(function (applicationUser) {
                    $scope.params.applicationUser = applicationUser;
                    if (applicationUser.roomId) {
                        $scope.params.roomId = applicationUser.roomId;
                        $scope.params.floorId = applicationUser.room.floorId;
                    } else {
                        $scope.params.roomId = null;
                        $scope.params.floorId = null;
                    }

                    $scope.loadData();
                });
            } else if ($scope.params.roomId) {
                roomRepository.get({ id: $location.search().roomId, includeProperties: 'Floor,RoomType' }).then(function (room) {
                    $scope.params.floorId = room.floorId;
                    $scope.params.room = room;
                    $scope.loadData();
                });
            } else {
                $scope.params.floorId = $state.params.floorId || 'default';
                $scope.loadData();
            }
        }

        function prepareForMap(floor) {
            if (!floor) {
                return;
            }

            if ($scope.params.roomId) {
                angular.forEach($scope.floor.rooms, function (value, key) {
                    if (value.id === $scope.params.roomId && authService.identity.isAuthenticated) {
                        value.selected = true;
                    }
                });
            }

            if ($scope.params.coords) {
                var coords = $scope.params.coords.split(',');
                coords = { x: coords[0], y: coords[1] };
                $scope.floor.pinCoords = coords;
            }
        }
    }
})();
