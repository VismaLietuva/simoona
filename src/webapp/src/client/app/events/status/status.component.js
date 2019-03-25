(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventStatus', {
            bindings: {
                event: '=',
                isDetails: '=',
                isAddColleague: '='
            },
            templateUrl: 'app/events/status/status.html',
            controller: eventStatusController,
            controllerAs: 'vm'
        });

    eventStatusController.$inject = [
        'eventStatus',
        'eventStatusService'
    ];

    function eventStatusController(eventStatus, eventStatusService) {
        /* jshint validthis: true */
        var vm = this;

        vm.eventStatus = eventStatus;
        vm.eventStatusService = eventStatusService;
    }
})();
