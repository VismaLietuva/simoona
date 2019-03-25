(function () {
    'use strict';

    angular.module('simoonaApp.RoomType')
        .controller('roomTypeManagerController', roomTypeManagerController);

    roomTypeManagerController.$inject = ['$scope', '$state', '$rootScope', 'roomTypeRepository', 'roomType',
        '$translate', 'notifySrv', 'localeSrv'];

    function roomTypeManagerController($scope, $state, $rootScope, roomTypeRepository, roomType,
        $translate, notifySrv, localeSrv) {

        init();

        function init() {
            $scope.state = {
                edit: $state.includes('Root.WithOrg.Admin.RoomTypes.Edit'),
                create: $state.includes('Root.WithOrg.Admin.RoomTypes.Create')
            };

            if ($scope.state.edit) {
                $rootScope.pageTitle = 'roomType.editRoomType';
            } else if ($scope.state.create) {
                $rootScope.pageTitle = 'roomType.createRoomType';
            }

            $scope.roomType = roomType;

            $scope.roomTypeColorStyle = {
                'background-color': roomType.color
            };
        }

        var createRoomType = function () {
            $scope.roomType.id = -1; // because 0 - allready exists as default room type.

            roomTypeRepository.create($scope.roomType)
                .then(function () {
                        notifySrv.success(localeSrv.formatTranslation('common.messageEntityCreated', {one: 'roomType.entityName', two: $scope.roomType.name}));
                        $state.go('^.List');
                    },
                    function (response) {
                        $scope.errors = response.data;
                    });
        };

        var uploadRoomType = function () {
            roomTypeRepository.update($scope.roomType).then(function () {

                    notifySrv.success(localeSrv.formatTranslation('common.messageEntityChanged', {one: 'roomType.entityName', two: $scope.roomType.name}));

                    $state.go('^.List');
                },
                function (response) {
                    $scope.errors = response.data;
                });
        };


        $scope.save = function () {
            if ($scope.state.create){
                $scope.create();
            }
            else if ($scope.state.edit) {
                $scope.update();
            }
        };

        $scope.create = function () {
            $scope.errors = [];
            createRoomType();
        };

        $scope.update = function () {
            $scope.errors = [];
            uploadRoomType();
        };
    };
})();
