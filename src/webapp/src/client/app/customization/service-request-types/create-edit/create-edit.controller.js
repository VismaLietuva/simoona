(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.ServiceRequestsTypes')
        .controller('serviceRequestsTypesCreateEditController', serviceRequestsTypesCreateEditController);

    serviceRequestsTypesCreateEditController.$inject = ['$rootScope', '$stateParams', '$state', 'serviceRequestsTypesRepository', 'errorHandler'];

    function serviceRequestsTypesCreateEditController($rootScope, $stateParams, $state, serviceRequestsTypesRepository, errorHandler) {
        /*jshint validthis: true */
        var vm = this;
        var listState = 'Root.WithOrg.Admin.Customization.ServiceRequestsTypes.List';

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Admin.Customization.ServiceRequestsTypes.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Customization.ServiceRequestsTypes.Edit')
        };
        
        vm.isLoading = vm.states.isCreate ? false : true;

        if (vm.states.isCreate) {
            $rootScope.pageTitle = 'customization.createServiceRequestsType';
        } else {
            $rootScope.pageTitle = 'customization.editServiceRequestsTypes';
        }

        vm.serviceRequestsType = {};
        vm.allUsers = allUsers;
        vm.createServiceRequestsType = createServiceRequestsType;
        vm.updateServiceRequestsType = updateServiceRequestsType;
        vm.disableServiceRequestsType = disableServiceRequestsType;
        vm.mapModel = mapModel;

        init();
        //////////

        function init() {
            if ($stateParams.id) {
                serviceRequestsTypesRepository.getServiceRequestsType($stateParams.id).then(function (type) {
                    vm.serviceRequestsType = type;
                    vm.isLoading = false;
                }, function (error) {
                    errorHandler.handleErrorMessage(error);
                    $state.go(listState);
                });
            }
        }

        function allUsers(search){
           return serviceRequestsTypesRepository.getUsers(search);
        }

        function createServiceRequestsType() {
            serviceRequestsTypesRepository.createServiceRequestsType(vm.serviceRequestsType).then(function() {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
        }

        function updateServiceRequestsType() {
            serviceRequestsTypesRepository.updateServiceRequestsType(vm.serviceRequestsType).then(function () {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
        }

        function disableServiceRequestsType() {
            serviceRequestsTypesRepository.disableType(vm.serviceRequestsType.id)
                .then(function () {
                    $state.go(listState);
                }, errorHandler.handleErrorMessage);
        }

        function mapModel(tag) {
            tag.userId = tag.id;
        }
    }
})();