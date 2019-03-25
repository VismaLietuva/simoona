(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('wallFeedController', wallFeedController);

    wallFeedController.$inject = [
        'authService',
        'wallService'
    ];

    function wallFeedController(authService, wallService) {
        /*jshint validthis: true */
        var vm = this;

        vm.hasAdministrationPermission = authService.hasPermissions(['POST_ADMINISTRATION']);
        vm.wallServiceData = wallService.wallServiceData;
    }
    
}());
