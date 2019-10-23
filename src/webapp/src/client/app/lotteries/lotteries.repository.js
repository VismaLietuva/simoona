(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .factory('lotteryRepository', lotteryRepository);

    lotteryRepository.$inject = [
        '$resource',
        '$http',
        'endPoint'
    ];

    function lotteryRepository($resource, $http, endPoint) {
        var url = endPoint + '/Lottery/';
        var lotteryWidgetUrl = endPoint + '/LotteryWidget/';

        var service = {
            getAllLotteries: getAllLotteries,
            getLottery: getLottery,
            create: create,
            updateDrafted: updateDrafted,
            updateStarted: updateStarted,
            revokeLottery: revokeLottery,
            getLotteryListPaged: getLotteryListPaged,
            finishLottery: finishLottery,
            getLotteryWidgetInfo: getLotteryWidgetInfo,
            buyTickets: buyTickets,
            getLotteryStatistics: getLotteryStatistics,
            exportParticipants: exportParticipants
        };
        return service;

        /////

        function getAllLotteries() {
            return $resource(url + 'All').query().$promise;
        }

        function getLottery(id) {
            return $resource(url + 'Details' + `?id=${id}`).get().$promise;
        }

        function create(lottery) {
            return $resource(url + 'Create').save(lottery).$promise;
        }

        function updateDrafted(lottery) {
            return $resource(url + 'UpdateDrafted', '', {
                put: {
                    method: 'PUT'
                }
                }).put(lottery).$promise;
        }

        function updateStarted(lottery) {
            return $resource(url + 'UpdateStarted', '', {
                patch: {
                    method: 'PATCH'
                }
            }).patch(lottery).$promise;
        }

        function revokeLottery(id) {
            return $resource(url + 'Abort').delete({id}).$promise;
        }

        function getLotteryListPaged(filters) {
            return $resource(url + 'Paged', '', {
                'query': {
                    method: 'GET',
                    isArray: false,
                    filters: filters
                }
            }).query(filters).$promise;
        }

        function finishLottery(id) {
            return $resource(url + 'Finish' + `?id=${id}`, '', {
                patch: {
                    method: 'PATCH'
                }
            }).patch().$promise;
        }
        function getLotteryWidgetInfo(){
            return $resource(lotteryWidgetUrl + 'Get')
                .query()
                .$promise;
        }
        function buyTickets(lotteryTickets) {
            return $resource(url + 'Enter').save(lotteryTickets).$promise;
        }

        function getLotteryStatistics(id) {
            return $resource(url + `${id}/Stats`).get().$promise;
        }
        function exportParticipants(lotteryId) {
            return $http.get(url + 'Export?lotteryId=' + lotteryId, {
                responseType: 'arraybuffer'
            });
        }
    }
})();
