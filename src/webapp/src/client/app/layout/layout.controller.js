(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout')
        .controller('layoutController', layoutController);

    layoutController.$inject = [
        'authService',
        'roles',
        '$window'
    ];

    function layoutController(authService, roles, $window) {
        /* jshint validthis: true */
        var vm = this;
        vm.isAuthenticated = authService.isAuthenticated;
        vm.isAuthenticatedNotNewUser = authService.isAuthenticatedNotNewUser;
        vm.isExternal = authService.isInRole(roles.external);
        vm.isChatBotEnabled = $window.isChatBotEnabled;
    }

})();
