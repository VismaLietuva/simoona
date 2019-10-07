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

        var service = {
            getAllLotteries: getAllLotteries,
            getLottery: getLottery,
            create: create
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
    }
})();
