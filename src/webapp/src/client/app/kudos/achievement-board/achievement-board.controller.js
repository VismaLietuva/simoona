(function() {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .constant('kudosAchievementBoardSettings', {
            chartOptions: {
                legend: { display: false }
            },
            colors: ['#0974B3'],
            series: ['Kudos'],
            firstTab: {
                months: 3,
                userCount: 10
            },
            secondTab: {
                months: 12,
                userCount: 10
            }
        })
        .controller('KudosAchievementBoard', KudosAchievementBoard);

    KudosAchievementBoard.$inject = [
        'kudosFactory',
        'kudosAchievementBoardSettings',
        'chartDataFactory',
        'errorHandler'
    ];

    function KudosAchievementBoard(kudosFactory, kudosAchievementBoardSettings, chartDataFactory, errorHandler) {
        /*jshint validthis: true */
        var vm = this;

        var boardSettings = kudosAchievementBoardSettings;

        vm.isLoading = true;

        vm.chartOptions = boardSettings.chartOptions;
        vm.chartColors = boardSettings.colors;
        vm.chartSeries = boardSettings.series;

        vm.kudosAchievementBoardData = {};
        vm.kudosAchievementBoardTabs = [{
            months: boardSettings.firstTab.months,
            userCount: boardSettings.firstTab.userCount,
            getStats: getFirstTabStats,
            isOpen: false
        }, {
            months: boardSettings.secondTab.months,
            userCount: boardSettings.secondTab.userCount,
            getStats: getSecondTabStats,
            isOpen: false
        }];

        init();

        function init() {
            getFirstTabStats();
        }

        function getFirstTabStats() {
            vm.isLoading = true;
            getKudosAchievmentData(boardSettings.firstTab, 0);
        }

        function getSecondTabStats() {
            vm.isLoading = true;
            getKudosAchievmentData(boardSettings.secondTab, 1);
        }

        function getKudosAchievmentData(settings, tabIndex) {
            for (var i = 0; i < vm.kudosAchievementBoardTabs.length; i++) {
                if (tabIndex === i) {
                    vm.kudosAchievementBoardTabs[i].isOpen = true;
                } else {
                    vm.kudosAchievementBoardTabs[i].isOpen = false;
                }
            }

            kudosFactory.getKudosStats(settings.months, settings.userCount).then(function(response) {
                vm.isLoading = false;
                vm.kudosAchievementBoardData = chartDataFactory.formatHorizontalChartData(response, 'name', 'kudosAmount');
            }, function(error) {
                vm.isLoading = false;
                errorHandler.handleErrorMessage(error);
            });
        }
    }
})();
