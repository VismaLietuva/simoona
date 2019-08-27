(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .factory('kudosFactory', kudosFactory);

    kudosFactory.$inject = [
        '$resource',
        '$http',
        'endPoint'
    ];

    function kudosFactory($resource, $http, endPoint) {
        var url = endPoint + '/Kudos/';

        var service = {
            getKudosPieChartData: getKudosPieChartData,
            approveKudos: approveKudos,
            rejectKudos: rejectKudos,
            getUsersForAutoComplete: getUsersForAutoComplete,
            getUserInformation: getUserInformation,
            getApprovedKudosList: getApprovedKudosList,
            getLastApprovedKudos: getLastApprovedKudos,
            getPointsTypes: getPointsTypes,
            getKudosStatuses: getKudosStatuses,
            getKudosFilteringTypes: getKudosFilteringTypes,
            getKudosLogs: getKudosLogs,
            getKudosTypeId: getKudosTypeId,
            getKudosStats: getKudosStats,
            getKudosWidgetStats: getKudosWidgetStats,
            exportKudosLog: exportKudosLog
        };
        return service;

        /////

        function getKudosPieChartData(userId) {
            return $resource(url + 'KudosPieChartData').query({
                userId: userId
            }).$promise;
        }

        function approveKudos(kudosLogId) {
            return $resource(url + 'ApproveKudos', {
                id: kudosLogId
            }).save().$promise;
        }

        function rejectKudos(kudosLogId, kudosRejectionMessage) {
            return $resource(url + 'RejectKudos', '', {
                post: {
                    method: 'POST'
                }
            }).post({
                id: kudosLogId,
                kudosRejectionMessage: kudosRejectionMessage
            }).$promise;
        }

        function getUsersForAutoComplete(searchString) {
            return $resource(url + 'GetUsersForAutocomplete').query({
                s: searchString
            }).$promise;
        }

        function getUserInformation(id) {
            return $resource(url + 'GetUserKudosInformationById', {}, {
                'GET': {
                    method: 'GET',
                    cache: true
                }
            }).get({
                id: id
            }).$promise;
        }

        function getApprovedKudosList(userId, filter) {
            return $resource(url + 'GetUserKudosLogs').get({
                userId: userId,
                page: filter.page
            }).$promise;
        }

        function getLastApprovedKudos(count) {
            return $resource(url + 'GetLastKudosLogRecords').query({
                count: count
            }).$promise;
        }

        function getPointsTypes() {
            return $resource(url + 'GetKudosTypesForFilter').query().$promise;
        }

        function getKudosStatuses() {
            return $resource(url + 'GetKudosStatuses').query().$promise;
        }
        
        function getKudosFilteringTypes() {
            return $resource(url + 'GetKudosFilteringTypes').query().$promise;
        }

        function getKudosLogs(filter) {
            return $resource(url + 'GetKudosLogs').get(filter).$promise;
        }

        function getKudosTypeId(kudosTypeName) {
            return $resource(url + 'getKudosTypeId').get({
                kudosTypeName: kudosTypeName
            }).$promise;
        }

        function getKudosStats(months, amount) {
            return $resource(url + 'GetKudosStats', {}, {
                'query': {
                    isArray: true
                }
            }).query({
                months: months,
                amount: amount
            }).$promise;
        }

        function getKudosWidgetStats(tabOneMonths, tabOneAmount, tabTwoMonths, tabTwoAmount) {
            return $resource(url + 'GetKudosWidgetStats', {}, {
                'query': {
                    isArray: true
                }
            }).query({
                tabOneMonths: tabOneMonths,
                tabOneAmount: tabOneAmount,
                tabTwoMonths: tabTwoMonths,
                tabTwoAmount: tabTwoAmount
            }).$promise;
        }

        function exportKudosLog(filter) {
            return $http.get(url + 'GetKudosLogAsExcel', { params: filter, responseType: 'arraybuffer' });
        }
    }
})();
