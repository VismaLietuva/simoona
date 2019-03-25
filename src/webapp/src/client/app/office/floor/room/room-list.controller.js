(function () {
    "use strict";

    angular.module('simoonaApp.Office.Floor.Room')
        .controller('roomController', roomController);

    roomController.$inject = [
        '$scope',
        'roomRepository',
        'userRepository',
        'rooms',
        '$state',
        '$rootScope',
        '$uibModal',
        'roomManageService',
        'mapRepository',
        '$document',
        'authService',
        '$translate',
        'notifySrv',
        '$timeout',
        'localeSrv'
    ];

    function roomController($scope, roomRepository, userRepository, rooms, $state,
     $rootScope, $uibModal, roomManageService, mapRepository, $document, authService, $translate, notifySrv, $timeout, localeSrv) {
        $scope.rooms = rooms;

        $rootScope.pageTitle = 'floor.entityName';

        $scope.allowEdit = authService.hasPermissions(['ROOM_ADMINISTRATION']);

        $scope.isInProgress = false;

        if ($state.params.floorId === null) {
            $scope.floorId = false;
        } else {
            $scope.floorId = true;
            mapRepository.getByFloor($state.params.floorId).then(function (model) {
                $scope.floor = model.floor;
                $scope.office = model.office;
            });
        }

        $scope.filter = {
            page: $state.params.page,
            dir: $state.params.dir,
            sort: $state.params.sort,
            s: $state.params.s,
            floorId: $state.params.floorId
        };

        $scope.$on('roomClicked', function (e, room) {
            if ($rootScope.isHandDrawingEnabled())
                return;

            userRepository.getByRoom({
                roomId: room.id
            }).then(function (data) {
                room.applicationUsers = data;
                $scope.manageRoom('Edit', room);
            });
        });

        $scope.getRooms = function () {
            var search = {};

            if ($scope.filter.floorId) {
                search.floorId = $scope.filter.floorId;
            }

            if ($scope.filter.sort !== null) {
                search.sort = $scope.filter.sort;
                search.dir = $scope.filter.dir;
            }

            if ($scope.filter.page !== null) {
                search.page = $scope.filter.page;
            }

            if ($scope.filter.s) {
                search.s = $scope.filter.s;
            }

            var filterWithIncludes = angular.extend({
                includeProperties: "ApplicationUsers,RoomType,Floor,Floor.Office"
            }, search);

            roomRepository.getPaged(filterWithIncludes).then(function (response) {
                $scope.rooms = response;
            });
            $scope.rooms = roomRepository.getPaged(filterWithIncludes);
            //$state.go('Root.WithOrg.Admin.Offices.Floors.Rooms.List', search, { reload: true });
        }

        $scope.onSort = function (sort, dir) {
            $scope.filter.dir = dir;
            $scope.filter.sort = sort;
            $scope.filter.page = 1;
            $scope.getRooms();
        }

        $scope.onSearch = function (search) {
            $scope.filter.s = search;
            $scope.filter.page = 1;
            $scope.getRooms();
        }

        $scope.onReset = function () {
            $scope.onSearch('');
        }

        $scope.onDelete = function (room) {
            roomRepository.delete(room.id).then(function () {
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted', {one: 'room.entityName', two: room.name}));

                $scope.filter.page = $state.params.page;
                if ($scope.rooms.length === 1) {
                    $scope.filter.s = null;
                }

                $scope.getRooms();
            }, function (response) {
                notifySrv.error(response.data);
            });
        }

        $scope.escapeKeyPressed = function (evt) {
            // Ignore if it's not esc button.
            if (evt.which !== 27)
                return;

            // Ignore if user is not drawing.
            if (!$rootScope.isHandDrawingEnabled())
                return;

            roomManageService.cancel();

            // Since this is not an angular event we need to notify the scope that smth changed!
            $scope.$apply();
        }

        $document.bind('keydown', $scope.escapeKeyPressed);
        $scope.manageRoom = function (mode, room) {
            roomManageService.setRoomScope($scope);
            if (mode === 'Create') {
                $scope.editStage = false;
                roomManageService.enableProgress();
                $timeout(enableDrawing);
            }
            if (mode === 'Edit') {
                $scope.editStage = true;
                roomManageService.enableProgress();
                $rootScope.disableHandDrawing();
                roomManageService.hideInfoMessage();
                roomManageService.checkRedrawMode();
                roomManageService.disableRedrawMode();
                roomManageService.clearVariables(mode);
                roomManageService.openPopupFromOutside(room);
            }
        }

        function enableDrawing() {
            var mode = 'Create';
            $rootScope.enableHandDrawing();
            roomManageService.checkRedrawMode();
            roomManageService.disableRedrawMode();
            roomManageService.clearVariables(mode);
            roomManageService.showInfoMessage();
        }
    }
})();
