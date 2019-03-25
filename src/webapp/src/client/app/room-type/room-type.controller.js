(function () {
    'use strict';

    angular.module('simoonaApp.RoomType')
        .controller('roomTypeController', roomTypeController);

    roomTypeController.$inject = ['$scope', '$state', '$rootScope', '$advancedLocation', '$uibModal', 'roomTypeRepository',
     'model', 'authService', '$translate', 'notifySrv', '$timeout', 'localeSrv'];

    function roomTypeController($scope, $state, $rootScope, $advancedLocation, $uibModal, roomTypeRepository,
     model, authService, $translate, notifySrv, $timeout, localeSrv) {



        $scope.reloadView = reloadView;
        $scope.ondelete = ondelete;
        $scope.onSearch = onSearch;
        $scope.onsort = onsort;
        $scope.onpagechanged = onpagechanged;

        $rootScope.pageTitle = 'roomType.entityNamePlural';

        $scope.roomTypes = model.pagedList;
        $scope.allowEdit = authService.hasPermissions(['ROOMTYPE_ADMINISTRATION']);

        $scope.filter = {
            page: $state.params.page,
            dir: $state.params.dir,
            sort: $state.params.sort,
            s: $state.params.s
        };

        activate();

        function activate() {
            reloadView(false);
        }

        function reloadView(reload) {
            $scope.pageCount = model.pageCount;
            $scope.itemCount = model.itemCount;
            $scope.pageSize = model.pageSize;

            var filter = {};

            if ($scope.filter.s !== undefined && $scope.filter.s !== null && $scope.filter.s !== "")
                filter.s = $scope.filter.s;

            if ($scope.filter.page != null)
                filter.page = $scope.filter.page;

            if ($scope.filter.sort !== undefined && $scope.filter.sort != null && $scope.filter.sort !== "") {
                filter.sort = $scope.filter.sort;
                filter.dir = $scope.filter.dir;
            }

            if (reload) {
                roomTypeRepository.getPaged(filter).then(function (model) {
                    $scope.roomTypes = model.pagedList;
                    $scope.pageCount = model.pageCount;
                    $scope.itemCount = model.itemCount;
                    $scope.pageSize = model.pageSize;
                }, function () {});
                $advancedLocation.search(filter);
            }
        }

        function ondelete(roomType) {
            roomTypeRepository.delete(roomType.id).then(function () {
                if ($scope.roomTypes.length === 1) {
                    if ($state.params.page > 1) {
                        $state.params.page = parseInt($state.params.page) - 1;
                    } else {
                        $state.params.s = null;
                    }
                }

                $scope.filter.page = $state.params.page;
                $scope.reloadView(true);

                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted', {one: 'roomType.entityName', two: roomType.name}));

                if ($scope.roomTypes.length === 1) {
                    $scope.filter.s = null;
                }

                reloadView(true);
            });
        };

        function onSearch(search) {
            $scope.filter.s = search;
            $scope.filter.page = 1;
            reloadView(true);
        }

        function onsort(sort, dir) {
            $scope.filter.dir = dir;
            $scope.filter.sort = sort;
            reloadView(true);
        }

        function onpagechanged() {
            reloadView(true);
        }
    };
})();
