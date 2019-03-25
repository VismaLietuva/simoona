(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('wallItemController', wallItemController);

    wallItemController.$inject = [
        '$state',
        'wallRepository',
        'wallService'
    ];

    function wallItemController($state, wallRepository, wallService) {
        /*jshint validthis: true */
        var vm = this;

        vm.wallServiceData = wallService.wallServiceData;
        vm.getCurrentWallId = wallService.getCurrentWallId;
        vm.stateParams = $state.params;

        init();

        /////////

        function init() {
            if (!!vm.stateParams.wall) {
                wallService.getWallDetails(vm.stateParams.wall);
            } else {
                vm.wallServiceData.wallHeader = null;
            }
        }
    }
}());
