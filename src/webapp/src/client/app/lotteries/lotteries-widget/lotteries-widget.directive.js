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
            'lotteryFactory',
            'lotteryStatus'
        ];

        function lotteriesWallWidget(authService, lotteryFactory, lotteryStatus){
            var directive = {
                restrict: 'E',
                templateUrl: 'app/lotteries/lotteries-widget/lottery-widget.html',
                link: linkFunc
            };
            return directive;

            function linkFunc(scope) {
                scope.lotteryStatus = lotteryStatus;
                scope.latestLotteries = [];
                scope.hasLotteryPermisions = authService.hasPermissions(['LOTTERY_BASIC']);

                getLotteryWidgetInfo();

                ////////

                function getLotteryWidgetInfo(){
                    lotteryFactory.getLotteryWidgetInfo().then(function(result) {
                        scope.latestLotteries = result;
                        console.log(result);
                    })
                }

            }
        }
})();