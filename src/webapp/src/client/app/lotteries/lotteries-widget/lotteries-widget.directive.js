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
                    })
                }
            }
            
        }

        function getTimespan(input) {
            if (!input) {
                return '';
            }
        var timestamp = new Date(input);

        if (timestamp == 'Invalid Date') {
            return '(invalid date)';
        }

        var now = new Date();

        var year = timestamp.getFullYear();

        if (year < 1970) {
            return (now.getFullYear() - year) + ' years ago';
        }

        var seconds = ((now.getTime() - timestamp) * .001) >> 0;

        if (seconds < 0) {
            seconds = Math.abs(seconds);
        }

        var minutes = seconds / 60;
        var hours = minutes / 60;
        var days = hours / 24;
        var years = days / 365;

        if (seconds < 10) {
            return 'a few seconds';
        }
        else if (seconds < 60) {
            return Math.round(seconds) + ' seconds';
        }
        else if (minutes < 1.5) {
            return 'a minute';
        }
        else if (minutes < 60) {
            return Math.round(minutes) + ' minutes';
        }
        else if (hours < 1.5) {
            return 'an hour';
        }
        else if (hours < 24) {
            return Math.round(hours) + ' hours';
        }
        else if (hours < 42) {
                return 'tomorrow';
        }
        else if (days < 30) {
            return Math.round(days) + ' days';
        }
        else if (days < 45) {
            return 'a month';
        }
        else if (days < 365) {
            return Math.round(days / 30) + ' months';
        }
        else if (years < 1.5) {
            return 'a year';
        }
        else {
            return Math.round(years) + ' years' ;
        }
        }
})();