(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout')
        .controller('layoutController', layoutController);

    layoutController.$inject = [
        'authService',
        'roles',
        'chatBotEndpoint',
        'chatBotAgentId'
    ];

    function layoutController(authService, roles, chatBotEndpoint, chatBotAgentId) {
        /* jshint validthis: true */
        var vm = this;

        vm.isAuthenticated = authService.isAuthenticated;
        vm.isAuthenticatedNotNewUser = authService.isAuthenticatedNotNewUser;
        vm.isExternal = authService.isInRole(roles.external);
        vm.isChatBotEnabled = chatBotEndpoint && chatBotAgentId;
    }

})();
