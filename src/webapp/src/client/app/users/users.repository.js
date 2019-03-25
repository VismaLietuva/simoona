(function () {
    'use strict';

    angular
        .module('simoonaApp.Users')
        .factory('userRepository', userRepository);

    userRepository.$inject = ['$resource', '$http', 'endPoint'];

    function userRepository($resource, $http, endPoint) {
        var applicationUserUrl = endPoint + '/ApplicationUser/';
        var officeMapUrl = endPoint + '/Map/';

        var service = {
            getByRoom: getByRoom,
            getByUserName: getByUserName,
            getPagedByFloor: getPagedByFloor,
            getUsersListPaged: getUsersListPaged,
            getJobTitleForAutoComplete: getJobTitleForAutoComplete,
            deleteItem: deleteItem,
            getForAutocomplete: getForAutocomplete,
            getManagersForAutocomplete: getManagersForAutocomplete,
            getUsersExcel: getUsersExcel
        };
        return service;

        ////////////

        function getByUserName(userName, includeProperties) {
            return $resource(applicationUserUrl + 'GetByUserName', '', {
                'query': {
                    method: 'GET',
                    isArray: false
                }
            }).query({
                userName: userName,
                includeProperties: includeProperties
            }).$promise;
        }

        function getByRoom(params) {
            params.includeProperties = 'QualificationLevel,JobPosition';
            return $resource(applicationUserUrl + 'GetByRoom', '', {
                'query': {
                    method: 'GET',
                    isArray: true
                }
            }).query(params).$promise;
        }

        function getPagedByFloor(params) {
            params.includeProperties = 'Room,JobPosition';
            return $resource(officeMapUrl + 'getPagedByFloor', '', {
                'query': {
                    method: 'GET',
                    params: params
                }
            }).query(params).$promise;
        }

        function getUsersListPaged(params) {
            return $resource(applicationUserUrl + 'GetPaged', '', {
                'query': {
                    method: 'GET',
                    isArray: false,
                    params: params
                }
            }).query(params).$promise;
        }

        function getJobTitleForAutoComplete(query) {
            return $resource(applicationUserUrl + 'GetJobTitleForAutoComplete').query(query).$promise;
        }

        function deleteItem(applicationUserId) {
            return $resource(applicationUserUrl + 'Delete').delete({
                id: applicationUserId
            }).$promise;
        }

        function getForAutocomplete(searchString) {
            return $resource(applicationUserUrl + 'GetForAutocomplete').query({
                s: searchString
            }).$promise;
        }

        function getManagersForAutocomplete(s, forWhoThisListIsUserId) {
            return $resource(applicationUserUrl + 'GetManagersForAutocomplete', '', {
                'query': {
                    method: 'GET',
                    isArray: true
                }
            }).query({
                s: s,
                userId: forWhoThisListIsUserId
            }).$promise;
        }

        function getUsersExcel() {
            return  $http.get(applicationUserUrl + 'GetUsersAsExcel', {
                responseType: 'arraybuffer'
            });
        }
    }
})();