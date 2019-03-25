(function() {
    'use strict';
    angular
        .module('simoonaApp.ServiceRequest')
        .controller('ServiceRequestController', ServiceRequestController);

    ServiceRequestController.$inject = [
        '$rootScope',
        '$scope',
        '$stateParams',
        '$uibModal',
        'authService',
        'serviceRequestRepository',
        'pagedServiceRequestList',
        'dataHelper',
        'errorHandler',
        'notifySrv',
        'lodash'
    ];

    function ServiceRequestController($rootScope, $scope, $stateParams, $uibModal, authService,
        serviceRequestRepository, pagedServiceRequestList, dataHelper, errorHandler, notifySrv, lodash) {       

        var openNewRequest = $stateParams.openNewRequest;
        $scope.edit = false;
        $scope.serviceRequest = {};
        $scope.newComment = {};
        $scope.setCategoryToKudos = false;
        $scope.pagedServiceRequestList = pagedServiceRequestList;
        $scope.getServiceRequestsList = getServiceRequestsList;
        $scope.onSort = onSort;
        $scope.onSearch = onSearch;
        $scope.changedPage = changedPage;
        $scope.onNewRequestButtonClick = onNewRequestButtonClick;
        $scope.editServiceRequestButtonClick = editServiceRequestButtonClick;
        $scope.closeServiceRequestButtonClick = closeServiceRequestButtonClick;
        $scope.closeKudosServiceRequestButtonClick = closeKudosServiceRequestButtonClick;
        $scope.exportButtonClick = exportButtonClick;
        $scope.openRequestDescriptionModal = openRequestDescriptionModal;
        $scope.cancel = cancel;
        $scope.hasEditableServiceRequests = false;

        $scope.userId = authService.identity.userId;
        $scope.isAdmin = authService.hasPermissions(['SERVICEREQUESTS_ADMINISTRATION']);

        $rootScope.pageTitle = 'serviceRequest.serviceRequests';

        $scope.filter = {
            includeProperties: 'Priority, Status, Employee',
            page: 1,
            sortOrder: 'desc',
            sortBy: 'Created',
            search: '',
            priority: '',
            status: '',
            serviceRequestCategory: ''
        };

        init();

        ////////////

        function init() {
            if (openNewRequest) {
                onNewRequestButtonClick(true);
            }

            if (dataHelper.isIdParamValid($stateParams.Id)) {
                serviceRequestRepository.get($stateParams.Id).then(function(response) {
                    editServiceRequestButtonClick(response);
                }, errorHandler.handleErrorMessage);
            }

            $scope.getServiceRequestsList();

            serviceRequestRepository.getCategories().then(function(response) {
                $scope.categories = response;
            }, errorHandler.handleErrorMessage);

            serviceRequestRepository.getPriorities().then(function(response) {
                $scope.priorities = response;
            }, errorHandler.handleErrorMessage);

            serviceRequestRepository.getStatuses().then(function(response) {
                $scope.statuses = response;
            }, errorHandler.handleErrorMessage);
        }

        function getServiceRequestsList() {
            serviceRequestRepository.getPaged($scope.filter).then(function(response) {
                $scope.pagedServiceRequestList = response;
                $scope.hasEditableServiceRequests = hasEditableServiceRequests(response.pagedList);
            }, errorHandler.handleErrorMessage);
        }

        function onSort(sortBy, sortOrder) {
            $scope.filter.sortOrder = sortOrder;
            $scope.filter.sortBy = sortBy;
            $scope.filter.page = 1;
            $scope.getServiceRequestsList();
        }

        function onSearch(search) {
            $scope.filter.page = 1;
            $scope.filter.search = search;
            $scope.getServiceRequestsList();
        }

        function changedPage() {
            $scope.getServiceRequestsList();
        }

        function onNewRequestButtonClick(setCategoryToKudos) {
            $scope.edit = false;
            $scope.serviceRequest = {};
            $scope.setCategoryToKudos = setCategoryToKudos;
            $uibModal.open({
                templateUrl: 'app/service-request/service-request-new-modal.html',
                controller: 'newRequestModalController',
                scope: $scope,
                backdrop: 'static'
            });
        }

        function exportButtonClick() {
            serviceRequestRepository.exportServiceRequests().then(function(response) {
                var file = new Blob([response.data], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;'
                });
                saveAs(file, 'service_requests.xlsx');
            }, errorHandler.handleErrorMessage);
        }

        function editServiceRequestButtonClick(serviceRequest) {
            $scope.edit = true;
            $scope.serviceRequest = serviceRequest;
            $uibModal.open({
                templateUrl: 'app/service-request/service-request-new-modal.html',
                controller: 'newRequestModalController',
                scope: $scope,
                backdrop: 'static'
            });
        }

        function closeServiceRequestButtonClick(serviceRequest) {

            $scope.serviceRequest = serviceRequest;

            $scope.serviceRequest.status = getDoneStatus();

            serviceRequestRepository.putToDone({ id: $scope.serviceRequest.id }).then(onSuccess, onError);
        }

        function closeKudosServiceRequestButtonClick(serviceRequest) {
            $scope.serviceRequest = serviceRequest;

            $uibModal.open({
                templateUrl: 'app/service-request/minus-kudos-modal.html',
                controller: 'minusKudosModalController',
                scope: $scope,
                backdrop: 'static'
            });
        }

        function openRequestDescriptionModal(serviceRequest) {
            $scope.serviceRequest = serviceRequest;

            $uibModal.open({
                templateUrl: 'app/service-request/service-request-description-modal.html',
                controller: 'serviceRequestDescriptionModalController',
                scope: $scope
            });
        }

        function getDoneStatus() {
            return lodash.find($scope.statuses, function(status) {
                return status.title === 'Done';
            });
        }

        function cancel() {
            $uibModal.close();
        }

        function onSuccess() {
            notifySrv.success('common.infoSaved');
            $scope.newComment.content = null;
        }

        function onError(response) {
            notifySrv.error(response.data.message);
            $scope.newComment.content = null;
        }
        
        function hasEditableServiceRequests(requests) {
            var editableFields = 0;
            angular.forEach(requests, function(request) {
                if (request.isCloseable) {
                    editableFields++;
                }
            });   
            return editableFields > 0;
        }
    }


})();
