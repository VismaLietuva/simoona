(function () {
    'use strict';

    angular.module('simoonaApp.Profile')
        .controller('profilesDetailsController', profilesDetailsController);

    profilesDetailsController.$inject = [
        '$rootScope',
        '$scope',
        '$state',
        'model',
        'authService',
        '$translate',
        'appConfig',
        '$stateParams',
        'roleRepository',
        'profileRepository',
        'notifySrv',
        'impersonate',
        '$window'
    ];

    function profilesDetailsController($rootScope, $scope, $state, model, authService, $translate,
        appConfig, $stateParams, roleRepository, profileRepository, notifySrv, impersonate, $window) {

        $scope.errors = [];

        $scope.homeStateName = appConfig.homeStateName;

        $scope.model = model;
        $rootScope.pageTitle = 'applicationUser.entityName';

        $scope.isAdmin = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);
        $scope.hasBlacklistPermission = authService.hasPermissions(['BLACKLIST_BASIC']);
        $scope.identity = authService.identity;
        $scope.isCurrentUser = $stateParams.id === authService.identity.userId;
        $scope.isNewUser = isNewUser;
        $scope.isFirstLogin = isFirstLogin;
        $scope.confirmUser = confirmUser;
        $scope.isPremium = $window.isPremium;
        $scope.showBlacklistHistory = $scope.hasBlacklistPermission && $scope.model.userWasPreviouslyBlacklisted;
        $scope.showBlacklistInformation = ($scope.isCurrentUser || $scope.hasBlacklistPermission) && $scope.model.blacklistEntry;
        $scope.blacklistHistoryExpanded = false;
        //$scope.submitJobInfo = submitJobInfo;

        $scope.isImpersonationEnabled = impersonate;

        $scope.model.displayName = displayedName;
        $scope.minLength = 1;
        $scope.hasLunch = hasLunch;

        $scope.redirectToTab = ($scope.isAdmin || $scope.isCurrentUser) ? 'Personal' : 'Blacklist';


		if($scope.model.dailyMailingHour) {
			$scope.model.dailyMailingHour = moment.utc($scope.model.dailyMailingHour, 'HH:mm:ss').local().format('HH:mm');
		}

        function defineFullTime() {
            if ($scope.model.workingHours && $scope.model.workingHours.fullTime) {
                $scope.workFullTime = 'yes';
            } else {
                $scope.workFullTime = 'no';
            }
        }
        defineFullTime();

        function isFirstLogin() {
            for (var i = 0; i < model.roles.length; i++) {
                if (model.roles[i].name === 'FirstLogin') {
                    return true;
                }
            }

            return false;
        }

        function isNewUser() {
            for (var i = 0; i < model.roles.length; i++) {
                if (model.roles[i].name === 'NewUser') {
                    return true;
                }
            }

            return false;
        }

        function confirmUser(id) {
            profileRepository.confirmUser(id).then(onSuccess, function (error) {
                notifySrv.error(error.data.message);
            });
        }

        function displayedName() {
            return $scope.model.firstName && $scope.model.lastName ?
                $scope.model.firstName + ' ' + $scope.model.lastName : $scope.model.userName;
        }

        function hasLunch() {
            return !!($scope.model.workingHours ?
                $scope.model.workingHours.lunchStart || $scope.model.workingHours.lunchEnd : false);
        }

        function onSuccess() {
            notifySrv.success('common.infoSaved');
            //profileRepository.getDetails($scope.model).then(function (response) {
            //    $scope.model = response;
            //}, onErrorResponse);
            $state.transitionTo($state.current, $stateParams, {
                reload: true,
                inherit: false,
                notify: true
            });
        }

        function onError(response) {
            angular.forEach(response.data, function (value) {
                notifySrv.error(value);
            });
        }

        function onErrorResponse(response) {
            notifySrv.error = response.data;
        }

        $scope.impersonate = function impersonate() {
            authService.impersonate(model.username);
        };

        //function confirmUser() {
        //    getRole("User").then(function (role) {
        //        model.jobInfo.roles.push(role);
        //        for (var i = 0; i < model.jobInfo.roles.length; i++) {
        //            if (model.jobInfo.roles[i].name === "NewUser") {
        //                model.jobInfo.roles.splice(i, 1);
        //                return submitJobInfo(true);
        //            }
        //        }
        //    }, onErrorResponse);
        //}

        //function submitJobInfo(isConfirm) {

        //    var jobInfo = angular.copy(model.jobInfo);

        //    jobInfo.certificateIds = mapIds(jobInfo.certificates);
        //    jobInfo.examIds = mapIds(jobInfo.exams);
        //    jobInfo.projectIds = mapIds(jobInfo.projects);
        //    jobInfo.skillIds = mapIds(jobInfo.skills);
        //    jobInfo.roleIds = mapIds(jobInfo.roles);
        //    jobInfo.isConfirm = isConfirm;

        //    jobInfo.managerId = jobInfo.manager
        //        ? jobInfo.manager.id
        //        : null;

        //    jobInfo.qualificationLevelId = jobInfo.qualificationLevel
        //        ? jobInfo.qualificationLevel.id
        //        : null;

        //    profileRepository.putJobInfo(jobInfo).then(onSuccess, onError);

        //    function onSuccess() {
        //        notifySrv.success('common.infoSaved');
        //    }

        //    function onError(response) {
        //        notifySrv.error(response.data);
        //    }

        //    function mapIds(array) {
        //        return array
        //            ? array.map(function (a) { return a.id })
        //            : [];
        //    }
        //}
    }
})();
