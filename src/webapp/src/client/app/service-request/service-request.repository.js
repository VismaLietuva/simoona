(function() {
    'use strict';

    angular
        .module('simoonaApp.ServiceRequest')
        .factory('serviceRequestRepository', serviceRequestRepository);

    serviceRequestRepository.$inject = ['$resource', '$http', 'endPoint'];

    function serviceRequestRepository($resource, $http, endPoint) {
        var serviceRequestUrl = endPoint + '/ServiceRequests/';

        var service = {
            get: get,
            getPaged: getPaged,
            getCategories: getCategories,
            getPriorities: getPriorities,
            getStatuses: getStatuses,
            getComments: getComments,
            post: post,
            postComment: postComment,
            put: put,
            putToDone: putToDone,
            shopItemsExist: shopItemsExist,
            getShopItems: getShopItems,
            exportServiceRequests: exportServiceRequests
        };

        return service;

        function get(id) {
            return $resource(serviceRequestUrl + 'Get', '', {
                'query': {
                    method: 'GET',
                    isArray: false
                }
            }).query({
                id: id,
                includeProperties: 'Priority, Status, Employee'
            }).$promise;
        }

        function getPaged(params) {
            var serviceCategoryName = '';
            
            if (!!params.serviceRequestCategory) {
                serviceCategoryName = params.serviceRequestCategory.name;
            }

            var parameters = {
                includeProperties: params.includeProperties,
                page: params.page,
                sortOrder: params.sortOrder,
                sortBy: params.sortBy,
                search: params.search,
                priority: params.priority,
                status: params.status,
                serviceRequestCategory: serviceCategoryName
            };
            return $resource(serviceRequestUrl + 'GetPagedFiltered', '', { 'query': { method: 'GET', isArray: false, params: parameters } }).query(parameters).$promise;
        }

        function shopItemsExist() {
            return $resource(serviceRequestUrl + 'KudosShopItemsExist').get().$promise;
      }
        
        function getShopItems(params) {
              return $resource(serviceRequestUrl + 'GetKudosShopItems', '', { 'query': { method: 'GET', isArray: true, params: params } }).query(params).$promise;
        }

        function getCategories(params) {
            return $resource(serviceRequestUrl + 'GetCategories', '', { 'query': { method: 'GET', isArray: true, params: params } }).query(params).$promise;
        }

        function getPriorities(params) {
            return $resource(serviceRequestUrl + 'GetPriorities', '', { 'query': { method: 'GET', isArray: true, params: params } }).query(params).$promise;
        }

        function getStatuses(params) {
            return $resource(serviceRequestUrl + 'GetStatuses', '', { 'query': { method: 'GET', isArray: true, params: params } }).query(params).$promise;
        }

        function getComments(params) {
            return $resource(serviceRequestUrl + 'GetComments', '', { 'query': { method: 'GET', isArray: true, params: { requestId: params } } }).query(params).$promise;
        }

        function post(model) {
            return $resource(serviceRequestUrl + 'Create').save(model).$promise;
        }

        function postComment(model) {
            return $resource(serviceRequestUrl + 'PostComment').save(model).$promise;
        }

        function put(model) {
            return $resource(serviceRequestUrl + 'Update', '', { Put: { method: 'PUT' } }).Put(model).$promise;
        }

        function putToDone(model) {
            return $resource(serviceRequestUrl + 'MarkAsDone', model, { Put: { method: 'PUT' } }).Put().$promise;
        }

        function exportServiceRequests() {
            return $http.get(serviceRequestUrl + 'GetServiceRequestsAsExcel', {
                responseType: 'arraybuffer'
            });
        }
    }
})();
