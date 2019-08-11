(function () {
    'use strict';

    angular.module('simoonaApp.Picture.Modal')
        .controller('pictureModalController', pictureModalController);

    pictureModalController.$inject = ['$scope', '$uibModalInstance', 'pictureId'];

    function pictureModalController($scope, $uibModalInstance, pictureId) {
        $scope.pictureId = pictureId;

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };
    }
})();
