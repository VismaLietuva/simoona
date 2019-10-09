(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .factory('lotteryFactory', lotteryFactory);

    lotteryFactory.$inject = [
        '$resource',
        '$http',
        'endPoint'
    ];

    function lotteryFactory($resource, $http, endPoint) {
        var url = endPoint + '/Lottery/';
        var lotteryWidgetUrl = endPoint + '/LotteryWidget/';

        var service = {
            getAllLotteries: getAllLotteries,
            getLotteriesByStatus: getLotteriesByStatus,
            getLottery: getLottery,
            create: create,
            updateDrafted: updateDrafted,
            updateStarted: updateStarted,
            revokeLottery: revokeLottery,
            getLotteryWidgetInfo: getLotteryWidgetInfo
        };
        return service;

        /////

        function getAllLotteries() {
            return $resource(url + 'All').query().$promise;
        }

        function getLotteriesByStatus(status){
            return $resource(url + 'Lotteries').query({
                status: status
            }).$promise;
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
            return $resource(url + 'Delete').delete({id}).$promise;
        }
        function getLotteryWidgetInfo(){
            return $resource(lotteryWidgetUrl + 'Get')
                .query()
                .$promise;
        }
    }
})();
