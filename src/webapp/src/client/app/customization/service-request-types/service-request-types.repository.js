(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.ServiceRequestsTypes')
        .factory('serviceRequestsTypesRepository', serviceRequestsTypesRepository);

    serviceRequestsTypesRepository.$inject = ['$resource', 'endPoint'];

    function serviceRequestsTypesRepository($resource, endPoint) {
        var serviceRequestsUrl = endPoint + '/ServiceRequests/';

        var service = {
            getServiceRequestsTypes: getServiceRequestsTypes,
            createServiceRequestsType: createServiceRequestsType,
            updateServiceRequestsType: updateServiceRequestsType,
            getServiceRequestsType: getServiceRequestsType,
            disableType: disableType,
            getUsers: getUsers
        };

        return service;
        
        ///////////
        function getUsers (params) {
            return $resource(endPoint + '/ApplicationUser/GetForAutoComplete').query({ s: params }).$promise;
        }
        function getServiceRequestsTypes() {
            return $resource(serviceRequestsUrl + 'GetServiceRequestCategories').query().$promise;
        }

        function createServiceRequestsType(type) {
            return $resource(serviceRequestsUrl + 'CreateCategory', '').save(type).$promise;
        }

        function updateServiceRequestsType(type) {
            return $resource(serviceRequestsUrl + 'EditCategory', '', {
                put: {
                    method: 'PUT'
                }
            }).put(type).$promise;
        }

        function getServiceRequestsType(id) {
            return $resource(serviceRequestsUrl + 'EditCategory').get({ categoryId: id }).$promise;
        }

        function disableType(id) {
            return $resource(serviceRequestsUrl + 'RemoveCategory', { categoryId: id }).delete().$promise;
        }
    }
})();