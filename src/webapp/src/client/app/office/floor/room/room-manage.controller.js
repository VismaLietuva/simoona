(function () {
    "use strict";

    angular.module('simoonaApp.Office.Floor.Room')
        .controller('roomManageController', roomManageController);

    roomManageController.$inject = ['$scope', '$uibModalInstance', 'model', 'roomManageService', 'floorid', 'roomRepository',
     'editStage', 'userRepository', 'roomTypeRepository', 'notifySrv', '$translate', 'authService', 'localeSrv'];

    function roomManageController($scope, $uibModalInstance, model, roomManageService, floorid, roomRepository,
     editStage, userRepository, roomTypeRepository, notifySrv, $translate, authService, localeSrv) {
        $scope.model = model;
        $scope.editStage = editStage;

        $scope.closePopupOnDemand = function () {
            roomManageService.checkRedrawMode();
            roomManageService.disableRedrawMode();
            roomManageService.clearVariables('Create');
            roomManageService.disableProgress();
            //$scope.dismiss();
            $uibModalInstance.close();
        };

        $scope.onRedraw = function (room) {
            var original = roomManageService.getOriginalRoom();
            if ((original.id === undefined) || (original === null)) {
                angular.extend(original, room);
            }
            roomManageService.onRedraw(original);
            $uibModalInstance.close();
        };

        $scope.onCreate = function () {
            //$scope.deselectRoom(model);

            $scope.model.floorId = floorid;
            if ($scope.model.roomTypeId === undefined) {
                $scope.model.roomTypeId = 1;
            }

            $scope.model.drawing = null;
            $scope.model.mark = null;

            roomRepository.create($scope.model).then(function () {
                    roomManageService.reloadPage();
                    notifySrv.success(localeSrv.formatTranslation('common.messageEntityCreated', {one: 'room.entityName', two: $scope.model.name}));
                    $uibModalInstance.close();
                },
                function (response) {
                    $scope.errors = response.data;
                });
        };

        $scope.onEdit = function () {
            //$scope.deselectRoom(model);
            $scope.model.floorId = floorid;

            $scope.model.drawing = null;
            $scope.model.mark = null;


            roomRepository.update(model).then(function (response) {
                    $uibModalInstance.close();
                    roomManageService.reloadPage();
                    notifySrv.success(localeSrv.formatTranslation('common.messageEntityChanged', {one: 'room.entityName', two: $scope.model.name}));
                    //$uibModalInstance.close();
                    roomManageService.deleteOriginalDrawing();
                },
                function (response) {
                    $scope.errors = response.data;
                });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        $scope.loadApplicationUsers = function (search) {
            return userRepository.getForAutocomplete(search);
        };

        if (authService.hasPermissions(['ROOMTYPE_ADMINISTRATION'])) {
            roomTypeRepository.getAll().then(function (data) {
                $scope.roomTypes = data;
            });
        }
    }
})();
