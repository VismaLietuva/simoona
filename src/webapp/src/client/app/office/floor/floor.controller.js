(function () {

    'use strict';

    angular.module('simoonaApp.Office.Floor')
        .controller('floorController', floorController);

    floorController.$inject = [
        '$scope',
        '$rootScope',
        '$state',
        '$uibModal',
        'floorRepository',
        'model',
        '$location',
        '$advancedLocation',
        'authService',
        '$translate',
        'notifySrv',
        'localeSrv',
        '$window'
    ];

    function floorController($scope, $rootScope, $state, $uibModal, floorRepository, model,
     $location, $advancedLocation, authService, $translate, notifySrv, localeSrv, $window) {
        $scope.model = model;
        
        $rootScope.pageTitle = 'office.entityName';

        $scope.allowEdit = authService.hasPermissions(['FLOOR_ADMINISTRATION']);
        $scope.hasRoomAdminPermission = authService.hasPermissions(['ROOM_ADMINISTRATION']);
        $scope.isPremium = $window.isPremium;

        $scope.filter = {
            page: $state.params.page,
            dir: $state.params.dir,
            sort: $state.params.sort,
            s: $state.params.s,
            officeId: $state.params.officeId
        };

        $scope.onSort = function (sort, dir) {
            $scope.filter.dir = dir;
            $scope.filter.sort = sort;
            $scope.filter.page = 1;
            $scope.getFloors();
        };

        $scope.onSearch = function (value) {
            $scope.filter.s = value;
            $scope.filter.page = 1;
            $scope.getFloors();
        }

        $scope.onReset = function () {
            $scope.filter.s = '';
            $scope.onSearch('');
        }

        $scope.onDelete = function (floor) {
            floorRepository.delete(floor.id).then(function () {
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted', {one: 'floor.entityName', two: floor.name}));
                $scope.filter.page = $state.params.page;
                if ($scope.model.pagedList.length === 1) {
                    $scope.filter.s = null;
                }
                $scope.getFloors();
            }, function (response) {
                notifySrv.error(response.data);
            });
        }


        $scope.openPictureModal = function (pictureId) {
            $uibModal.open({
                templateUrl: 'app/picture/picture-modal/picture-modal-content.html',
                size: 'lg',
                controller: 'pictureModalController',
                resolve: {
                    pictureId: function () {
                        return pictureId;
                    }
                }
            });
        };

        $scope.getFloors = function () {
            var filter = {};
            if ($scope.filter.officeId) {
                filter.officeId = $scope.filter.officeId;
            }

            if ($scope.filter.sort && $scope.filter.sort.length) {
                filter.sort = $scope.filter.sort;
                if ($scope.filter.dir && $scope.filter.dir.length) {
                    filter.dir = $scope.filter.dir;
                }
            }

            if ($scope.filter.page) {
                filter.page = $scope.filter.page;
            }

            if ($scope.filter.s) {
                filter.s = $scope.filter.s;
            }

            var filterWithIncludes = angular.extend({
                includeProperties: "ApplicationUsers,RoomType,Floor,Floor.Office"
            }, filter);

            floorRepository.getPaged(filterWithIncludes).then(function (response) {
                $scope.model = response;
            });

            // NOTE: not working correctly
            //$state.go('Root.WithOrg.Admin.Offices.Floors.List', filter, { reload: true });
        };

        $scope.changePage = function () {
            $scope.getFloors();
        };
    }
})();
