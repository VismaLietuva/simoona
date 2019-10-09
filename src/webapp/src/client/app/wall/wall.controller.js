(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('wallController', wallController);

    wallController.$inject = [
        '$rootScope',
        'wallRepository'
    ];

    function wallController($rootScope, wallRepository) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'wall.wallTitle';

        //init
        vm.widgetsInfo = {};
        vm.lotteryWidgetInfo = {};

        wallRepository.getWidgetsInfo()
            .then(function(widgetsInfo) { 
                vm.widgetsInfo = widgetsInfo;
            }); 
    }
}());
