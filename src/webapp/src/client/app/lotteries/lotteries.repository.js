(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .factory('lotteryRepository', lotteryRepository);

    lotteryRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function lotteryRepository($resource, endPoint) {
        var url = endPoint + '/Lottery/';
        var lotteryWidgetUrl = endPoint + '/LotteryWidget/';

        var service = {
            getAllLotteries: getAllLotteries,
            getLottery: getLottery,
            create: create,
            updateDrafted: updateDrafted,
            updateStarted: updateStarted,
            abortLottery: abortLottery,
            getLotteryListPaged: getLotteryListPaged,
            finishLottery: finishLottery,
            getLotteryStatus: getLotteryStatus,
            refundParticipants: refundParticipants,
            getLotteryWidgetInfo: getLotteryWidgetInfo,
            buyTickets: buyTickets,
            getLotteryStatistics: getLotteryStatistics,
            exportParticipants: exportParticipants,
            getLotteryParticipants: getLotteryParticipants
        };
        return service;


        function getAllLotteries() {
            return $resource(url + 'All').query().$promise;
        }

        function getLottery(id) {
            return $resource(url + `${id}/Details`).get().$promise;
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

        function abortLottery(id) {
            return $resource(url + `${id}/Abort`, '', {
                patch: {
                    method: 'PATCH'
                }
            }).patch().$promise;
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
            return $resource(url + `${id}/Finish`, '', {
                patch: {
                    method: 'PATCH'
                }
            }).patch().$promise;
        }

        function getLotteryStatus(id) {
            return $resource(url + `${id}/Status`).get().$promise;
        }

        function refundParticipants(id) {
            return $resource(url + `${id}/Refund`, '', {
                patch: {
                    method: 'PATCH'
                }
            }).patch().$promise;
        }
<<<<<<< HEAD

=======
>>>>>>> master
        function getLotteryWidgetInfo(){
            return $resource(lotteryWidgetUrl + 'Get')
                .query().$promise;
        }

        function buyTickets(lotteryTickets) {
            return $resource(url + 'Enter').save(lotteryTickets).$promise;
        }

        function getLotteryStatistics(id) {
            return $resource(url + `${id}/Stats`).get().$promise;
        }
<<<<<<< HEAD
        
        function exportParticipants(lotteryId) {
            return $http.get(url + 'Export?lotteryId=' + lotteryId, {
                responseType: 'arraybuffer'
            });
=======

        function getLotteryParticipants(filters) {
            return $resource(url + `Participants/Paged`, '', {
                'query': {
                    method: 'GET',
                    isArray: false
                }
            }).query(filters).$promise;
>>>>>>> master
        }
    }
})();
