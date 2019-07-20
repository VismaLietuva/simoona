(function() {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosAchievementBoardWidget', {
            bindings: {
                tabs: '=',
                tabonemonths: '=',
                taboneamount: '=',
                tabtwomonths: '=',
                tabtwoamount: '='
            },
            templateUrl: 'app/kudos/achievement-board-widget/achievement-board-widget.html',
            controller: kudosAchievementBoardController,
            controllerAs: 'vm'
        });

    function kudosAchievementBoardController() {
        /*jshint validthis: true */
        var vm = this;
        vm.tabs = [];
    }
}());
