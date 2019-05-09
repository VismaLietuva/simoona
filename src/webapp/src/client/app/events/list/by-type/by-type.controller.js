(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('eventsSettings', {
            eventsListPageSize: 20
        })
        .controller('eventsByTypeController', eventsByTypeController);

    eventsByTypeController.$inject = [
        '$stateParams',
        'eventRepository',
        'authService',
        'eventsSettings'
    ];

    function eventsByTypeController($stateParams, eventRepository, authService, eventsSettings) {
        /*jshint validthis: true */
        var vm = this;

        vm.addMoreEvents = addMoreEvents;

        vm.isEventsFound = true;
        vm.isEventsLoading = true;
        vm.eventsList = [];
        vm.itemsDisplayedInList = eventsSettings.eventsListPageSize;

        init();

        ///////////

        function init() {
            if ($stateParams.type === 'all' && $stateParams.office === 'all') {
                eventRepository.getAllEvents().then(function (result) {
                    vm.eventsList = result;

                    setResponseUtilities(result);
                });
            } else if ($stateParams.type === 'host' || $stateParams.type === 'participant') {
                getMyEvents($stateParams.type);
            } else {
                eventRepository.getEventsByTypeAndOffice($stateParams.type, $stateParams.office).then(function (result) {
                    vm.eventsList = result;
                    setResponseUtilities(result);
                });
            }
        }

        function getMyEvents(typeId) {
            eventRepository.getMyEvents({
                filter: typeId
            }).then(function (result) {
                vm.eventsList = result;
                setResponseUtilities(result);
            });
        }

        function setResponseUtilities(data) {
            vm.isEventsFound = !!data.length;
            vm.isEventsLoading = false;
        }

        function addMoreEvents() {
            if (vm.itemsDisplayedInList < vm.eventsList.length) {
                vm.itemsDisplayedInList = vm.itemsDisplayedInList + eventsSettings.eventsListPageSize;
            }
        }
    }
})();
