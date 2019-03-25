(function () {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .controller('logOffController', logOffController);

    logOffController.$inject = ['authService', 'notificationHub'];

    function logOffController(authService, notificationHub) {
        notificationHub.disconnectFromHub();
        authService.logOut();
    }
})();
