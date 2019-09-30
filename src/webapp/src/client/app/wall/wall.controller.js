(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .constant('minDisplaySizes', {
            rightSidebar: 992
        })
        .controller('wallController', wallController);

    wallController.$inject = [
        '$rootScope',
        '$window',
        '$timeout',
        'minDisplaySizes',
        'wallRepository'
    ];

    function wallController($rootScope, $window, $timeout, minDisplaySizes, wallRepository) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'wall.wallTitle';

        //init
        vm.rightBarMinSize = minDisplaySizes.rightSidebar;
        vm.windowWidth = $window.innerWidth;
        vm.widgetsInfo = {};
        wallRepository.getWidgetsInfo()
            .then(function (widgetsInfo) {
                vm.widgetsInfo = widgetsInfo;
            });

        $window.onresize = function () {
            $timeout(function () {
                vm.windowWidth = $window.innerWidth;
            });
        };
    }
}());
