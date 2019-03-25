(function () {
    "use strict";

    angular.module('simoonaApp.Office')
        .controller('officeListController', officeListController);

    officeListController.$inject = ['$scope', '$state', '$rootScope', '$uibModal', '$advancedLocation', 'officeRepository',
     'offices', 'authService', 'notifySrv', '$timeout', '$translate', 'localeSrv'];

    function officeListController($scope, $state, $rootScope, $uibModal, $advancedLocation, officeRepository,
     offices, authService, notifySrv, $timeout, $translate, localeSrv) {

        $scope.isLoading = false;


        $scope.changedPage = changedPage;
        $scope.onSort = onSort;
        $scope.onReset = onReset;
        $scope.onSearch = onSearch;
        $scope.onDelete = onDelete;
        $scope.hasFloorAdminPermission = authService.hasPermissions(['OFFICE_ADMINISTRATION']);

        $rootScope.pageTitle = 'office.entityNamePlural';

        $scope.headers = [{
            title: 'office.name',
            value: 'Name'
        }, {
            title: 'office.country',
            value: 'Country'
        }, {
            title: 'office.city',
            value: 'City'
        }, {
            title: 'office.address',
            value: 'Building'
        }];

        $scope.modalShown = false;

        $scope.allowEdit = authService.hasPermissions(['OFFICE_ADMINISTRATION']);

        $scope.filter = {
            page: $state.params.page,
            dir: $state.params.dir,
            sort: $state.params.sort,
            s: $state.params.s,
        };

        $scope.offices = offices.pagedList;
        $scope.pageCount = offices.pageCount;
        $scope.itemCount = offices.itemCount;
        $scope.pageSize = offices.pageSize;
        officeRepository.setOffices(offices);

        $scope.startLoading = function () {
            $scope.loader = $timeout(function () {
                $scope.isLoading = true;
            }, 150);
        }

        $scope.stopLoading = function () {
            $timeout.cancel($scope.loader);
            $scope.isLoading = false;
        }

        function changeState() {
            var search = {};
            if ($scope.filter.sort !== null) {
                search.sort = $scope.filter.sort;
                search.dir = $scope.filter.dir;
            }

            if ($scope.filter.page !== null)
                search.page = $scope.filter.page;

            if ($scope.filter.s !== null && $scope.filter.s !== '' && $scope.filter.s !== undefined)
                search.s = $scope.filter.s;

            var filterWithIncludes = angular.extend({
                includeProperties: "Floors,Floors.Rooms,Floors.Rooms.ApplicationUsers"
            }, search);
            $scope.startLoading();
            officeRepository.getPaged(filterWithIncludes).then(function (model) {
                $scope.stopLoading();
                $scope.offices = model.pagedList;
                $scope.pageCount = model.pageCount;
                $scope.itemCount = model.itemCount;
                officeRepository.setOffices(model);
            }, function () {
                $scope.stopLoading();
            });
            //$advancedLocation.search(search);
        }

        function changedPage() {
            changeState(officeRepository);
        }

        function onSort(sort, dir) {
            $scope.filter.dir = dir;
            $scope.filter.sort = sort;
            $scope.filter.page = 1;
            changeState(officeRepository);
        }

        function onSearch(search) {
            $scope.filter.s = search;
            $scope.filter.page = 1;
            changeState(officeRepository);
        }

        function onReset() {
            $scope.onSearch('');
        }

        function onDelete(office) {
            officeRepository.delete(office.id).then(function () {
                if ($scope.offices.length === 1) {
                    if ($state.params.page > 1) {
                        $state.params.page = parseInt($state.params.page) - 1;
                    } else {
                        $state.params.s = null;
                    }
                }
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted', {one: 'office.entityName', two: office.name}));

                $scope.filter.page = $state.params.page;
                if ($scope.offices.length === 1) {
                    $scope.filter.s = null;
                }

                changeState(officeRepository);
            });
        }
    };
})();
