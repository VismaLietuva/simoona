(function () {
    'use strict';

    angular.module('simoonaApp.ServiceRequest')
        .controller('serviceRequestDescriptionModalController', serviceRequestDescriptionModalController);

    serviceRequestDescriptionModalController.$inject = ['$scope', '$uibModalInstance', 'serviceRequestRepository'];

    function serviceRequestDescriptionModalController($scope, $uibModalInstance, serviceRequestRepository) {
        $scope.comments = [];
        
        serviceRequestRepository.getComments($scope.serviceRequest.id).then(function (response) {
            $scope.comments = response;
        });
        
        $scope.cancel = function () {
            $uibModalInstance.close();
        };
    }

})();