(function () {
    'use strict';

    angular
        .module('simoonaApp.Users')
        .controller('applicationUserListController', applicationUserListController);

    applicationUserListController.$inject = [
        '$rootScope',
        '$state',
        '$advancedLocation',
        '$window',
        'userRepository',
        'skillRepository',
        'projectRepository',
        'usersModel',
        'authService',
        '$translate',
        'notifySrv',
        'localeSrv',
        'endPoint'
    ];

    function applicationUserListController($rootScope, $state, $advancedLocation, $window, userRepository,
        skillRepository, projectRepository, usersModel, authService, $translate, notifySrv, localeSrv, endPoint) {
        /*jshint validthis: true */
        const vm = this;

        const filterObject = !!$state.params.filter ? JSON.parse($state.params.filter) : [];

        vm.currentUser = authService.identity.userName;
        vm.usersModel = usersModel;
        vm.allowEdit = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);
        vm.customFilterIsOpen = false;

        vm.getExcel = getExcel;
        vm.triggerItemFiltering = triggerItemFiltering;
        vm.changedPage = changedPage;
        vm.onSort = onSort;
        vm.onFilteringItemList = onFilteringItemList;
        vm.okCallback = okCallback;
        vm.deleteItem = deleteItem;

        $rootScope.pageTitle = 'applicationUser.entityNamePlural';

        vm.customFilter = {
            header: 'applicationUser.filter',
            filterValues: filterObject,
            filters: [{
                label: 'applicationUser.jobTitle',
                usersModel: null,
                filter: 'jobtitle',
                placeholder: 'applicationUser.jobTitleInputTagPlaceHolder',
                load: function (query) {
                    return userRepository.getJobTitleForAutoComplete({
                        query: query
                    });
                }
            }, {
                label: 'applicationUser.projects',
                usersModel: null,
                filter: 'projects',
                placeholder: 'applicationUser.projectInputTagPlaceHolder',
                load: function (query) {
                    return projectRepository.getForAutocomplete({
                        name: query
                    });
                }
            }, {
                label: 'applicationUser.skills',
                usersModel: null,
                filter: 'skills',
                placeholder: 'applicationUser.skillsInputTagPlaceHolder',
                load: function (query) {
                    return skillRepository.getForAutoComplete({
                        s: query
                    });
                }
            }]
        };

        vm.filter = {
            page: $state.params.page,
            dir: $state.params.dir,
            sort: $state.params.sort ?? 'userName',
            s: $state.params.s,
            filter: filterObject
        };

        function triggerItemFiltering(filters) {
            vm.customFilterIsOpen = true;
            vm.customFilter.filtersValues = filters;
            vm.filter.filter = filters;
            vm.filter.page = 1;
            changeState();
        }

        function changedPage() {
            changeState();
        }

        function onSort(sort, dir) {
            vm.filter.dir = dir;
            vm.filter.sort = sort;
            vm.filter.page = 1;
            changeState();
        }

        function onFilteringItemList(searchString) {
            vm.filter.s = searchString;
            vm.filter.page = 1;
            changeState();
        }

        function deleteItem(item) {
            userRepository.deleteItem(item.id).then(function () {
                if ($state.params.page > 1) {
                    $state.params.page = parseInt($state.params.page) - 1;
                } else {
                    $state.params.s = null;
                }
                okCallback($state.params.page, item);
                $advancedLocation.search($state.params);
            }, function (response) {
                if(response.status === 405) {
                    notifySrv.error('errorCodeMessages.userHasPendingKudos');
                } else {
                    notifySrv.error(response);
                }

            });

        }

        function okCallback(page, item) {
            var fullName = '';
            vm.filter.page = page;
            if (!!item.firstName && !!item.lastName) {
                fullName = item.firstName + ' ' + item.lastName;
            }

            if (!fullName) {
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeletedOneParam',
                    {one: 'applicationUser.entityName'}));
            } else {
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted',
                    {one: 'applicationUser.entityName', two: fullName}));
            }

            if (vm.usersModel.pagedList.length === 1) {
                vm.filter.s = null;
            }
            changeState();
        }

        function changeState() {
            const filterParams = {};

            if (!!vm.filter.sort) {
                filterParams.sort = vm.filter.sort;
                filterParams.dir = vm.filter.dir;
            }

            if (!!vm.filter.page) {
                filterParams.page = vm.filter.page;
            }

            if (!!vm.filter.s) {
                filterParams.s = vm.filter.s;
            }

            if (!!vm.filter.filter) {
                filterParams.filter = JSON.stringify(vm.filter.filter);
            }

            userRepository.getUsersListPaged(filterParams).then(function (usersModel) {
                vm.usersModel = usersModel;
            });

            $advancedLocation.search(filterParams);
        }

        function getExcel() {
            userRepository.getUsersExcel().success(function (response) {
                const file = new Blob([response], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;'
                });

                saveAs(file, 'Users.xlsx');
            });
        }
    }
})();
