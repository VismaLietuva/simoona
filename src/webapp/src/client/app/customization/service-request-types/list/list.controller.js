(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.ServiceRequestsTypes')
        .controller('serviceRequestsTypesListController', serviceRequestsTypesListController);

    serviceRequestsTypesListController.$inject = ['$rootScope', 'serviceRequestsTypesRepository', 'errorHandler'];

    function serviceRequestsTypesListController($rootScope, serviceRequestsTypesRepository, errorHandler) {
        /*jshint validthis: true */
        var vm = this;
        
        $rootScope.pageTitle = 'customization.serviceRequestsTypes';
        vm.serviceRequestsTypes = [];

        init();
        
        ///////////
        
        function init() {
            serviceRequestsTypesRepository.getServiceRequestsTypes()
                .then(function(types) {
                    vm.serviceRequestsTypes = types;
                }, errorHandler.handleErrorMessage);
        }
    }
})();