(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('wallController', wallController);

    wallController.$inject = [
        '$rootScope',
        '$window',
        'wallRepository'
    ];

    function wallController($rootScope, $window, wallRepository) {
        /*jshint validthis: true */
        var vm = this;
        vm.isChatBotEnabled = $window.isChatBotEnabled;
        $rootScope.pageTitle = 'wall.wallTitle';

        //init
        vm.widgetsInfo = {};

        wallRepository.getWidgetsInfo()
            .then(function(widgetsInfo) {
                vm.widgetsInfo = widgetsInfo;
            });
    }
}());
