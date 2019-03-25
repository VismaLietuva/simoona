(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceProgressBar', progressBar);

    function progressBar() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/progress-bar/progress-bar.html',
            scope: {
                value: '=',
                max: '=',
                fullDanger: '='
            },
            controller: progressBarController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    progressBarController.$inject = [
        '$scope'
    ];

    function progressBarController($scope) {
        /* jshint validthis: true */
        var vm = this;

        init();

        //////

        function init() {
            $scope.$watch('vm.value', function(value) {
                updateProgressBar(value, vm.max);
            });
        }

        function updateProgressBar(value, max) {
            vm.isFull = vm.fullDanger && (value >= max);
            vm.percents = parseFloat((value / max) * 100).toFixed(2);
            vm.percents = vm.percents > 100 ? 100 : vm.percents;
        }
    }
})();
