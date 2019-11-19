(function(){
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .constant("lotteryStatus", {
            Drafted: 1,
            Started: 2,
            Aborted: 3,
            Ended: 4
        })
        .directive('aceLotteriesWallWidget', lotteriesWallWidget);

        lotteriesWallWidget.$inject = [
            'authService',
            'lotteryRepository',
            'lotteryStatus',
            '$window'
        ];

        function lotteriesWallWidget(authService, lotteryRepository, lotteryStatus, $window){
            var directive = {
                restrict: 'E',
                templateUrl: 'app/lotteries/lotteries-widget/lotteries-widget.html',
                link: linkFunc,
                scope: {
                    latestLotteries: '=?',
                },
                controller: function(){
                    var vm = this;
                },
                controllerAs: 'vm'
            };
            return directive;

            function linkFunc(scope) {
                if(!$window.lotteriesEnabled)
                {
                    return;
                }
                scope.lotteryStatus = lotteryStatus;
                scope.latestLotteries = [];
                scope.hasLotteryPermisions = authService.hasPermissions(['LOTTERY_BASIC']);

                if(scope.$$prevSibling.hasLotteryPermisions)
                {
                    getLotteryWidgetInfo();
                }

                function getLotteryWidgetInfo(){
                    lotteryRepository.getLotteryWidgetInfo().then(function(result) {
                        scope.latestLotteries = result;
                        filterEndedLotteries();
                    })
                }
                function filterEndedLotteries(){
                    angular.forEach(scope.latestLotteries, (lottery) => {
                        if(moment.utc(lottery.endDate)-moment() < 0)
                        {
                            scope.latestLotteries.remove(lottery);
                        }
                    });
                }
            }
        }
})();
