(function () {
    'use strict';

    angular
        .module('simoonaApp')
        .factory('leftMenuService', leftMenuService);

    function leftMenuService() {

        var status = {
            isOpen : false
        };

        var service = {
            getStatus: getStatus,
            setStatus: setStatus
        };

        return service;

        /////

        function getStatus() {
            return status.isOpen;
        }

        function setStatus(isOpen) {
            status.isOpen = isOpen;
        }
    }
})();
