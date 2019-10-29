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
        .directive('aceLotteriesWallWidget', lotteriesWallWidget)
        .filter('timespan', function () {
            return getTimespan;
        });

        lotteriesWallWidget.$inject = [
            'authService',
            'lotteryRepository',
            'lotteryStatus',
            'localeSrv'
        ];

        function lotteriesWallWidget(authService, lotteryRepository, lotteryStatus, localeSrv){
            var directive = {
                restrict: 'E',
                templateUrl: 'app/lotteries/lotteries-widget/lotteries-widget.html',
                link: linkFunc
            };
            return directive;

            function linkFunc(scope) {
                scope.lotteryStatus = lotteryStatus;
                scope.latestLotteries = [];
                scope.getRemainingTime = getRemainingTime;
                scope.hasLotteryPermisions = authService.hasPermissions(['LOTTERY_BASIC']);

                getLotteryWidgetInfo();

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
            function getRemainingTime(input){

                var timeRemaining;
                var unitOfTime = "";
                if(!input){
                    return '';
                }

                var seconds = (moment.utc(input)-moment()) *.001;
                var minutes = seconds / 60;
                var hours = minutes / 60;
                var days = hours / 24;
                var years = days / 365;

                if(seconds < 10)
                {
                    unitOfTime = "fewSeconds";
                }
                else if(seconds < 60) {
                    timeRemaining = Math.round(seconds);
                    unitOfTime = "seconds";
                }
                else if(minutes < 1.5) {
                    unitOfTime = "minute";
                }     
                else if(minutes < 60) {
                    timeRemaining = Math.round(minutes);
                    unitOfTime = "minutes";
                }   
                else if(hours < 1.5) {
                    unitOfTime = "hour";
                }   
                else if(hours < 60) {
                    timeRemaining = Math.round(hours);
                    unitOfTime = "hours";
                }
                else if(hours < 42) {
                    unitOfTime = "tomorrow";
                }  
                else if(days < 30) {
                    timeRemaining = Math.round(days);
                    unitOfTime = "months";
                }  
                else if(days < 45) {
                    unitOfTime = "month";
                } 
                else if(days < 365) {
                    timeRemaining = Math.round(days / 30);
                    unitOfTime = "months";
                } 
                else if(years < 1.5) {
                    unitOfTime = "year";
                } 
                else {
                    timeRemaining = Math.round(years);
                    unitOfTime = "years";
                } 

                if(timeRemaining) {
                    return `${timeRemaining} ${localeSrv.translate('lotteries.' + unitOfTime)}`
                }
                else {
                    return localeSrv.translate('lotteries.' + unitOfTime);
                }
            }
            
        }

        function getTimespan(input) {
            if (!input) {
                return '';
            }
            return moment.utc(input).fromNow();
        }
})();
