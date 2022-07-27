(function () {
    'use strict';

    angular
        .module('simoonaApp.Profile')
        .constant('tabNames', {
            personal: {
                id: 0,
                name: 'personal'
            },
            job: {
                id: 1,
                name: 'job'
            },
            office: {
                id: 2,
                name: 'office'
            },
            blacklist: {
                id: 3,
                name: 'blacklist'
            },
        })
        .controller('profileEditController', profileEditController);

    profileEditController.$inject = [
        '$rootScope',
        '$scope',
        '$stateParams',
        'authService',
        'profileRepository',
        'notifySrv',
        'lodash',
        'tabName',
        'tabNames',
    ];

    function profileEditController(
        $rootScope,
        $scope,
        $stateParams,
        authService,
        profileRepository,
        notifySrv,
        lodash,
        tabName,
        tabNames
    ) {
        $rootScope.pageTitle = 'applicationUser.profile';

        $scope.hasOfficePermission = authService.hasPermissions([
            'OFFICE_BASIC',
            'FLOOR_BASIC',
            'ROOM_BASIC',
        ]);

        $scope.hasApplicationUserPermission = authService.hasPermissions([
            'APPLICATIONUSER_ADMINISTRATION',
        ]);

        $scope.hasBlacklistPermission = authService.hasPermissions([
            'BLACKLIST_ADMINISTRATION',
        ]);

        $scope.isCurrentUser = $stateParams.id === authService.identity.userId;
        $scope.tabNames = tabNames;

        $scope.personalInfo = {
            isLoaded: false
        };

        $scope.jobInfo = {
            isLoaded: false
        };

        $scope.officeInfo = {
            isLoaded: false
        };

        $scope.blacklistInfo = {
            isLoaded: false
        };

        $scope.isNewUser = authService.isInRole('NewUser');

        $scope.loadData = loadData;

        init();

        function init() {
            loadData(tabName);
            reapplyTabdrop();
        }

        function loadData(tabName) {
            $scope.tabName = tabName.toLowerCase();

            switch ($scope.tabName) {
                case tabNames.office.name:
                    setActiveTab(tabNames.office);
                    loadOnce($scope.officeInfo, loadProfileOffice);
                    break;
                case tabNames.personal.name:
                    setActiveTab(tabNames.personal);
                    loadOnce($scope.personalInfo, loadPersonalInfo);
                    break;
                case tabNames.blacklist.name:
                    setActiveTab(tabNames.blacklist);
                    loadOnce($scope.blacklistInfo, loadBlacklistInfo);
                    break;
                case tabNames.job.name:
                    setActiveTab(tabNames.job);
                    loadOnce($scope.jobInfo, loadJobInfo);
                    break;
                default:
                    console.error('Invalid tab name provided');
            }
        }

        function setActiveTab(tabName) {
            $scope.activeTab = tabName.id;
        }

        function loadOnce(infoObject, loadFunction) {
            if (infoObject.isLoaded) {
                return;
            }

            loadFunction();
        }

        function loadJobInfo() {
            profileRepository
                .getUserProfileJob($stateParams.id)
                .then(function (response) {
                    $scope.jobInfo = response;
                    $scope.jobInfo.isLoaded = true;

                    $scope.jobInfo.currentJobPosition = lodash.find(
                        response.jobPositions,
                        function (o) {
                            return o.isSelected === true;
                        }
                    );
                }, onError);
        }

        function loadBlacklistInfo() {
            profileRepository.getBlacklistEntry($stateParams.id).then(
                function (response) {
                    $scope.blacklistInfo = response;
                    $scope.blacklistInfo.isLoaded = true;
                },
                function () {
                    $scope.blacklistInfo.isLoaded = true;
                }
            );
        }

        function loadProfileOffice() {
            profileRepository
                .getUserProfileOffice($stateParams.id)
                .then(function (response) {
                    $scope.officeInfo = response;
                    $scope.officeInfo.isLoaded = true;
                }, onError);
        }

        function loadPersonalInfo() {
            profileRepository
                .getUserProfilePersonal($stateParams.id)
                .then(function (response) {
                    $scope.personalInfo = response;
                    $scope.personalInfo.isLoaded = true;
                }, onError);
        }

        function onError(response) {
            notifySrv.error(response.data);
        }

        function reapplyTabdrop() {
            setTimeout(function () {
                $('.nav.nav-tabs').tabdrop('layout');
            }, 100);
        }
    }
})();
