(function() {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosAchievementBoardWidget', {
            bindings: {
                tabonemonths: '=',
                taboneamount: '=',
                tabtwomonths: '=',
                tabtwoamount: '='
            },
            templateUrl: 'app/kudos/achievement-board-widget/achievement-board-widget.html',
            controller: kudosAchievementBoardController,
            controllerAs: 'vm'
        });

    kudosAchievementBoardController.$inject = [
        'kudosFactory',
        'errorHandler'
    ];

    function kudosAchievementBoardController(kudosFactory, errorHandler) {
        /*jshint validthis: true */
        var vm = this;

        vm.tabs = [];
        vm.isLoading = true;

        init();

        /////////

        function init() {
            kudosFactory.getKudosWidgetStats(vm.tabonemonths, vm.taboneamount, vm.tabtwomonths, vm.tabtwoamount).then(function(response) {
                vm.tabs = response;
                vm.isLoading = false;
            }, function(error) {
                errorHandler.handleErrorMessage(error);
                vm.isLoading = false;
            });
        }
    }
}());
