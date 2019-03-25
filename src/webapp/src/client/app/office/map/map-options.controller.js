(function () {

    'use strict';

    angular
        .module('simoonaApp.Map')
        .constant('mapOptionsSettings', {
            defaultEmailOptionIndex: 0
        })
        .controller('mapOptionsController', mapOptionsController);

    mapOptionsController.$inject = [
        '$scope',
        '$rootScope',
        'authService',
        'mapRepository',
        'officeRepository',
        'notifySrv',
        'mapOptionsSettings',
        '$translate',
        'Analytics'
    ];

    function mapOptionsController($scope, $rootScope, authService, mapRepository, officeRepository,
    notifySrv, mapOptionsSettings, $translate, Analytics) {
        $scope.emailOptions = [{
            optionId: 1,
            optionName: 'floor.emailOptionFloor',
            optionFunc: getFloorEmails
        }, {
            optionId: 2,
            optionName: 'floor.emailOptionOffice',
            optionFunc: getOfficeEmails
        }, {
            optionId: 3,
            optionName: 'floor.emailOptionRoom',
            optionFunc: getRoomEmails
        }];

        $scope.currentEmailsList = '';
        $scope.currentSelectedEmailOption = $scope.emailOptions[mapOptionsSettings.defaultEmailOptionIndex]; //default
        $scope.emailOptions[mapOptionsSettings.defaultEmailOptionIndex].optionFunc();
        $scope.isRoomSelected = false;

        $scope.$watch('selectedRoom.id', function (newValue, oldValue) {
            if ($scope.currentSelectedEmailOption.optionId === 3) {
                $scope.isRoomSelected = !(!!$scope.selectedRoom && $scope.currentSelectedEmailOption.optionId === 3 && !!$scope.currentEmailsList);
                if ($scope.selectedRoom && $scope.selectedRoom.id) {
                    getRoomEmails();
                }
            }

        }, true);

        $scope.changeOfficeSelection = function (currentOffice) {
            if ((!$scope.office || $scope.office.id !== currentOffice.id) && currentOffice.floors.length > 0) {
                $rootScope.$emit('floorChange', currentOffice.floors[0]);
            }
            $scope.office.id = currentOffice.id;
            $scope.floor.id = currentOffice.floors[0].id;
            if($scope.currentSelectedEmailOption.optionId === 2) {
                getOfficeEmails();
            } else if ($scope.currentSelectedEmailOption.optionId === 1) {
                getFloorEmails();
            }
        };

        $scope.changeFloorSelection = function (floor) {
            if ($scope.floor.id !== floor.id) {
                $rootScope.$emit('floorChange', floor);
            }
            $scope.floor.id = floor.id;
            if ($scope.currentSelectedEmailOption.optionId === 1) {
                getFloorEmails();
            }
        };

        function getFloorEmails() {
            $scope.currentSelectedEmailOption = $scope.emailOptions[0];
            $scope.isRoomSelected = false;

            if ($scope.floor) {
                officeRepository.getUsersEmailsByFloor($scope.floor.id).then(function (response) {
                    $scope.currentEmailsList = response.join();
                });
            }

        }

        function getOfficeEmails() {
            $scope.currentSelectedEmailOption = $scope.emailOptions[1];
            $scope.isRoomSelected = false;

            officeRepository.getUsersEmailsByOffice($scope.office.id).then(function (response) {
                $scope.currentEmailsList = response.join();

            });
        }

        function getRoomEmails() {
            $scope.currentSelectedEmailOption = $scope.emailOptions[2];
            $scope.isRoomSelected = !(!!$scope.selectedRoom && $scope.currentSelectedEmailOption.optionId === 3 && !!$scope.currentEmailsList);
            if ($scope.selectedRoom && $scope.selectedRoom.id) {
                officeRepository.getUsersEmailsByRoom($scope.selectedRoom.id).then(function (response) {
                    $scope.currentEmailsList = response.join();
                    $scope.isRoomSelected = !(!!$scope.selectedRoom && $scope.currentSelectedEmailOption.optionId === 3 && !!$scope.currentEmailsList);
                });
            }
        }

        $scope.addToClipboard = function () {
            Analytics.trackEvent('Office map', 'copy emails', 'addToClipboard');
            notifySrv.success('floor.successToClipboard');
        };
    }
})();
