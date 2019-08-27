(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .constant('kudosLogSettings', {
            maxCommentsLength: 500,
            minCommentsLength: 15
        })
        .controller('kudosLogListController', kudosLogListController);

    kudosLogListController.$inject = [
        'kudosFactory',
        'kudosLogSettings',
        'notifySrv',
        'errorHandler',
        'lodash'
    ];

    function kudosLogListController(kudosFactory, kudosLogSettings, notifySrv, errorHandler, lodash) {
        /*jshint validthis: true */
        var vm = this;

        vm.kudos = {};
        vm.statuses = {};
        vm.filterTypes={};

        vm.filter = {
            page: 1,
            searchUser: null,
            status: 'Pending',
            sortBy: 'Created',
            sortOrder: 'desc',
            filteringType: 'All'
        };

        vm.perPage = 0;
        vm.totalItems = 0;

        vm.isKudosLogsLoading = true;
        vm.isKudosStatusLoading = true;
        vm.isExcelLoading = false;

        vm.kudosLogSettings = kudosLogSettings;
        vm.modalReject = modalReject;
        vm.approveKudos = approveKudos;
        vm.getUsersForAutocomplete = getUsersForAutocomplete;
        vm.getColumnFilteredKudosLogList = getColumnFilteredKudosLogList;
        vm.getFilteredKudosLogList = getFilteredKudosLogList;
        vm.exportToExcel = exportToExcel;

        init();

        /////////

        function init() {

            kudosFactory.getKudosStatuses().then(function (response) {
                vm.statuses = response;
                vm.isKudosStatusLoading = false;
            }, function (error) {
                vm.isKudosStatusLoading = false;
                errorHandler.handleErrorMessage(error);
            });

            kudosFactory.getKudosFilteringTypes().then(function (response) {
                vm.filterTypes = response;
                vm.isKudosStatusLoading = false;
            }, function (error) {
                vm.isKudosStatusLoading = false;
                errorHandler.handleErrorMessage(error);
            });

            getFilteredKudosLogList();
        }

        function modalReject(kudosEntry) {
            updateLogList(kudosEntry);
        }

        function getUsersForAutocomplete(query) {
            return kudosFactory.getUsersForAutoComplete(query);
        }

        function approveKudos(kudosEntry) {
            if (!kudosEntry.isLoading) {
                kudosEntry.isLoading = true;
                kudosFactory.approveKudos(kudosEntry.id).then(function () {
                    updateLogList(kudosEntry);
                    kudosEntry.isLoading = false;
                }, function (error) {
                    kudosEntry.isLoading = false;
                    errorHandler.handleErrorMessage(error);
                });
            }
        }

        function updateLogList(kudosEntry) {
            lodash.remove(vm.kudos, kudosEntry);
            if (!vm.kudos.length) {
                reloadWhenNoElementLeft();
            } else {
                insertAdditionalElements(kudosEntry);
            }
        }

        function reloadWhenNoElementLeft() {
            if (vm.filter.page > 1) {
                vm.filter.page = vm.filter.page - 1;
                getFilteredKudosLogList();
            }
        }

        function insertAdditionalElements(kudosEntry) {
            var filter = filterKudos();
            kudosFactory.getKudosLogs(filter).then(function (response) {
                vm.totalItems = response.itemCount;
                vm.perPage = response.pageSize;

                var missingObjects = lodash.filter(response.pagedList, function (obj) {
                    return !lodash.find(vm.kudos, obj);
                });

                lodash.each(missingObjects, function (obj) {
                    vm.kudos.push(obj);
                });
            }, errorHandler.handleErrorMessage);
            
        }

        function getColumnFilteredKudosLogList(sortBy, sortOrder) {
            vm.filter.sortBy = sortBy;
            vm.filter.sortOrder = sortOrder;
            getFilteredKudosLogList();
        }

        function getFilteredKudosLogList() {
            vm.isKudosLogsLoading = true;
            var filter = filterKudos();
            kudosFactory.getKudosLogs(filter).then(function (response) {
                vm.kudos = response.pagedList;
                vm.totalItems = response.itemCount;
                vm.perPage = response.pageSize;
                vm.isKudosLogsLoading = false;
            }, function (error) {
                vm.isKudosLogsLoading = false;
                errorHandler.handleErrorMessage(error);
            });
        }

        function exportToExcel() {
            vm.isExcelLoading = true;
            var filter = filterKudos();
            kudosFactory.exportKudosLog(filter).then(function(response) {
                vm.isExcelLoading = false;
                var file = new Blob([response.data], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;'
                });
                saveAs(file, 'kudos_log.xlsx');
            }, function (error) {
                vm.isExcelLoading = false;
                errorHandler.handleErrorMessage(error);
            });
        }

        function filterKudos() {
            var filter = {
                searchUserId: vm.filter.searchUser ? vm.filter.searchUser.id : null,
                status: vm.filter.status,
                page: vm.filter.page,
                sortBy: vm.filter.sortBy,
                sortOrder: vm.filter.sortOrder,
                filteringType: vm.filter.filteringType
            };
            return filter;
        }
    }
})();