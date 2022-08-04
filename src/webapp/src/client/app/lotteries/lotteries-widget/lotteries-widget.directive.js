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
            '$window',
            '$location'
        ];

        function lotteriesWallWidget(authService, lotteryRepository, lotteryStatus, $window, $location){
            var directive = {
                restrict: 'E',
                templateUrl: 'app/lotteries/lotteries-widget/lotteries-widget.html',
                link: linkFunc
            };

            return directive;

            function linkFunc(scope) {
                if(!$window.lotteriesEnabled)
                {
                    return;
                }

                scope.lotteryStatus = lotteryStatus;
                scope.latestLotteries = [];
                scope.lotteryIdFromRoute = getLotteryIdFromUrlParams();
                scope.hasLotteryPermisions = authService.hasPermissions(['LOTTERY_BASIC']);

                scope.saveOpenLotteryWidgetFunction = saveOpenLotteryWidgetFunction;
                scope.openLotteryWidget = null;

                if(!scope.$root.lottery) {
                    getLotteryWidgetInfo();
                }
                else {
                    scope.$root.lottery = false;
                }

                function getLotteryWidgetInfo(){
                    lotteryRepository.getLotteryWidgetInfo().then(function(result) {
                        scope.latestLotteries = filterEndedLotteries(result);

                        tryToOpenLotteryWidget();
                    });

                    scope.$root.lottery = true;
                }

                function filterEndedLotteries(result) {
                    return result.filter(lottery => moment.utc(lottery.endDate) - moment() >= 0);
                }

                function getLotteryIdFromUrlParams() {
                    return $location.search().lotteryId;
                }

                function tryToOpenLotteryWidget() {
                    setTimeout(function() {
                        if (scope.openLotteryWidget) {
                            scope.openLotteryWidget();
                        }
                    });
                }

                /*
                * This function registers the latest initialized lottery widget open function to avoid opening multiple modals at the same time.
                * This directive is rendered twice in wall.html and because of that we get a lot of unnecessary initializations that make it difficult to open lottery widget once via code.
                */
                function saveOpenLotteryWidgetFunction(initializedLotteryId, openLotteryWidgetFunction) {
                    if (getLotteryIdFromUrlParams() == initializedLotteryId) {
                        scope.openLotteryWidget = openLotteryWidgetFunction;
                    }
                }
            }
        }
})();
