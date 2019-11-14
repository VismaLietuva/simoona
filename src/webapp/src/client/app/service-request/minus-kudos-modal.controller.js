(function () {
    'use strict';

    angular.module('simoonaApp.ServiceRequest')
        .controller('minusKudosModalController', minusKudosModalController);

    minusKudosModalController.$inject = ['$scope', '$uibModalInstance', 'kudosifyModalFactory', '$translate',
                                         'serviceRequestRepository', 'notifySrv', 'kudosifySettings', 'kudosFactory', 'errorHandler', 'lodash'];

    function minusKudosModalController($scope, $uibModalInstance, kudosifyModalFactory, $translate,
                                        serviceRequestRepository, notifySrv, kudosifySettings, kudosFactory, errorHandler, lodash) {

        $scope.maxKudosMinus = kudosifySettings.maxMinus;

        $scope.statuses = '';
        $scope.isDisabled = false;

        serviceRequestRepository.getStatuses().then(function (response) {
            $scope.statuses = response;
            $scope.serviceRequest.status = $scope.statuses[3];
        });

        serviceRequestRepository.getCategories().then(function (response) {
            $scope.serviceRequest.serviceRequestCategoryId = lodash.find(response, function (category) {
                    return category.name === $scope.serviceRequest.categoryName;
                }).id;
        });

        $scope.minusKudosModel = {
                comment: $scope.serviceRequest.title,
                multiplyBy: $scope.serviceRequest.kudosAmmount,
                pointsType: { name: 'Minus', value: 1, isActive:true },
                id: $scope.serviceRequest.employee.id
            };

        kudosFactory.getKudosTypeId('Minus').then(function (response) {
            $scope.minusKudosModel.pointsType.id = response.kudosTypeId;
        });

        $scope.closeKudosServiceRequest = function () {
            $scope.serviceRequest.statusId = $scope.serviceRequest.status.id;
            $scope.serviceRequest.priorityId = $scope.serviceRequest.priority.id;
            serviceRequestRepository.put($scope.serviceRequest).then(onSuccess, onError);
        };

        $scope.submitAutomaticComment = function () {
            if ($scope.minusKudosModel.multiplyBy !== $scope.serviceRequest.kudosAmmount) {
                $scope.newComment.content = $translate.instant('serviceRequest.automaticCommentMessage') + ' ' + $scope.minusKudosModel.multiplyBy;
                $scope.newComment.ServiceRequestId = $scope.serviceRequest.id;
                serviceRequestRepository.postComment($scope.newComment).then($scope.closeKudosServiceRequest, onError);
            } else {
                $scope.closeKudosServiceRequest();
            }
        };

        $scope.submitMinusKudos = function () {
            $scope.isDisabled = true;
            kudosifyModalFactory
                .postKudos([$scope.minusKudosModel.id], $scope.minusKudosModel, $scope.minusKudosModel.pointsType)
                .then($scope.submitAutomaticComment, errorHandler.handleErrorMessage);
        };



        function onSuccess() {
            notifySrv.success('common.infoSaved');
            $scope.newComment.content = null;
            $uibModalInstance.close();
        }

        function onError(response) {
            notifySrv.error(response.data.message);
            $scope.newComment.content = null;
        }

        $scope.cancel = function () {
            $uibModalInstance.close();
            $scope.getServiceRequestsList();
        };
    }
})();
