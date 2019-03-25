(function () {
    'use strict';

    angular.module('simoonaApp.Profile')
        .controller('profileEditController', profileEditController);

    profileEditController.$inject = [
        '$rootScope',
        '$scope',
        'model',
        '$translate',
        'authService',
        'tabName',
        'profileRepository',
        'notifySrv',
        '$timeout',
        'lodash'];

    function profileEditController($rootScope, $scope, model, $translate,
        authService, tabName, profileRepository, notifySrv, $timeout, lodash) {

        $rootScope.pageTitle = 'applicationUser.profile';

        $scope.hasOfficePermission = authService.hasPermissions(['OFFICE_BASIC', 'FLOOR_BASIC', 'ROOM_BASIC']);

        $scope.personalInfo = model;
        $scope.jobInfo = {};
        $scope.officeInfo = {};

        $scope.displayedName = displayedName;

        $scope.isNewUser = authService.isInRole('NewUser');

        function displayedName() {
            return $scope.personalInfo.firstName && $scope.personalInfo.lastName ?
                $scope.personalInfo.firstName + ' ' + $scope.personalInfo.lastName : $scope.personalInfo.userName;
        }

        $scope.onTabChange = function (tab) {
            if (tab === 'job' && !$scope.jobInfo.isLoaded) {
                loadData(tab);
            }

            if (tab === 'office' && !$scope.officeInfo.isLoaded) {
                loadData(tab);
            }
        };

        function loadData(tab) {
            if (tab === 'job') {
                profileRepository.getUserProfileJob($scope.personalInfo).then(function (response) {
                    $scope.jobInfo = response;
                    $scope.jobInfo.isLoaded = true;

                    $scope.jobInfo.currentJobPosition = 
                        lodash.find(response.jobPositions, function(o) { return o.isSelected === true; });
                    
                }, onError);
            }

            if (tab === 'office') {
                profileRepository.getUserProfileOffice($scope.personalInfo).then(function (response) {
                    $scope.officeInfo = response;
                    $scope.officeInfo.isLoaded = true;
                }, onError);
            }
        }

        function onError(response) {
            notifySrv.error(response.data);
        }
    }
})();