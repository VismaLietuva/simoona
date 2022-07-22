(function () {
    'use strict';

    angular.module('simoonaApp.Profile')
        .directive('profileBlacklist', profileBlacklist);

    profileBlacklist.$inject = [];

    function profileBlacklist() {

        return {
            templateUrl: 'app/profile/profile-edit/blacklist/user-profile-blacklist.html',
            restrict: 'AE',
            scope: {
                model: '='
            },
            controller: controller
        };
    }

    controller.$inject = [
        '$scope',
        '$timeout',
        '$stateParams',
        'authService',
        'notifySrv',
        'profileRepository'
    ];

    function controller($scope, $timeout, $stateParams, authService, notifySrv, profileRepository) {
        $scope.datePickers = {
            endDate: false
        };

        $scope.errors = [];
        $scope.isAdmin = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);

        $scope.openDatePicker = openDatePicker;
        $scope.onChangeValidateEndDate = onChangeValidateEndDate;
        $scope.saveInfo = saveInfo;
        $scope.deleteFromBlacklist = deleteFromBlacklist;

        $scope.endDateDisplay = undefined;
        $scope.minDate = getMinBlacklistingDate();

        // User is blacklisted if end date is set
        if ($scope.model.endDate) {
            $scope.allowDeletion = true;

            updateEndDateDisplay();
        }

        $scope.model.endDate = new Date($scope.model.endDate || getDefaultEndDate());

        function saveInfo() {
            var params = {
                userId: $stateParams.id,
                endDate: $scope.model.endDate,
                reason: $scope.model.reason
            };

            if ($scope.allowDeletion) {
                profileRepository
                    .putBlacklistState(params)
                    .then(function() {
                        updateEndDateDisplay();
                        showSuccess();
                    }, showError);
            } else {
                profileRepository.createBlacklistState(params)
                    .then(function() {
                        $scope.allowDeletion = true;
                        var fullNameParts = authService.identity.fullName.split(' ');
                        var timestamp = moment().local().startOf('days').toDate();

                        $scope.model.modifiedByUserFirstName = fullNameParts[0];
                        $scope.model.modifiedByUserLastName = fullNameParts[1];
                        $scope.model.createdByUserFirstName = fullNameParts[0];
                        $scope.model.createdByUserLastName = fullNameParts[1];
                        $scope.model.modified = timestamp;
                        $scope.model.created = timestamp;

                        updateEndDateDisplay();
                        showSuccess();
                    }, showError);
            }
        }

        function deleteFromBlacklist() {
            profileRepository.deleteBlacklistState({
                userId: $stateParams.id
            }).then(function() {
                $scope.allowDeletion = false;
                $scope.model.endDate = getDefaultEndDate();
                $scope.model.reason = '';

                showSuccess();
            }, showError)
        }

        function openDatePicker($event, key) {
            $event.preventDefault();
            $event.stopPropagation();

            closeAllDatePickers(key);

            $timeout(function() {
                $event.target.focus();
            }, 100);
        }

        function closeAllDatePickers(datePicker) {
            $scope.datePickers.endDate = false;
            $scope.datePickers[datePicker] = true;
        }

        function getMinBlacklistingDate() {
            return moment().local().startOf('days').add(1, 'days').toDate();
        }

        function getDefaultEndDate() {
            return moment($scope.minDate).add(2, 'year').toDate();
        }

        function showError() {
            notifySrv.error('errorCodeMessages.messageError');
        }

        function showSuccess() {
            notifySrv.success('common.infoSaved');
        }

        function onChangeValidateEndDate() {
            if ($scope.blacklistInfo.endDate) {
                $scope.blacklistInfo.endDate.$setValidity('minDate', $scope.model.endDate >= $scope.minDate);
            }
        }

        function updateEndDateDisplay() {
            $scope.endDateDisplay = moment($scope.model.endDate).toDate();
        }
    }
})();
