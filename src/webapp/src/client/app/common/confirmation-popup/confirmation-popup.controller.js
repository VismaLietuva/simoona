(function () {

    'use strict';

    angular
        .module('simoonaApp.Common.ConfirmationPopup')
        .controller('confirmationPopupController', confirmationPopupController);

    confirmationPopupController.$inject = ['$scope', '$uibModalInstance', '$state', '$advancedLocation', 'title', 'message', 'callback'];

    function confirmationPopupController($scope, $uibModalInstance, $state, $advancedLocation, title, message, callback) {
        $scope.title = title;
        $scope.message = message;

        $scope.cancel = function () {
            $uibModalInstance.close();
            callback(false);
        };

        $scope.ok = function () {
            $uibModalInstance.close();
            callback(true);
        }
    }
})();
