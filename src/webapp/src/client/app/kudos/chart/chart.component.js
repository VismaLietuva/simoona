(function () {
    'use strict';
    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosChart', {
            templateUrl: 'app/kudos/chart/chart.html',
            controller: kudosChartController,
            controllerAs: 'vm'
        });

    kudosChartController.$inject = [
        '$stateParams',
        'authService',
        'kudosFactory',
        'chartDataFactory',
        'errorHandler'
    ];

    function kudosChartController($stateParams, authService, kudosFactory, chartDataFactory, errorHandler) {
        /* jshint validthis: true */
        var vm = this;

        vm.isLoading = true;
        vm.kudosChartData = {};
        var userId = $stateParams.userId || authService.identity.userId;

        init();

        ////////

        function init() {
            kudosFactory.getKudosPieChartData(userId).then(function (result) {
                vm.isLoading = false;
                vm.kudosChartData = chartDataFactory.formatPieChartData(result, 'name', 'value');
            }, function(error) {
                vm.isLoading = false;
                errorHandler.handleErrorMessage(error);
            });
        }

    }
})();
