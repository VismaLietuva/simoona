(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .controller('eventsListController', eventsListController);

    eventsListController.$inject = [
        '$rootScope'
    ];

    function eventsListController($rootScope) {
        /*jshint validthis: true */
        var vm = this;
        $rootScope.pageTitle = 'events.eventsTitle';
    }
})();
