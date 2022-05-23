(function () {
    'use strict';
    angular
        .module('simoonaApp.Events')
        .controller('eventDetailsListController', eventDetailsListController);

    eventDetailsListController.$inject = [
        'notifySrv',
        'eventRepository',
        '$state',
    ];

    function eventDetailsListController(
        notifySrv,
        eventRepository,
        $state
    ) {
        var vm = this;

        vm.isLoadingControls = true;
        vm.isLoadingEvents = true;

        vm.page = 1;
        vm.filter = {
            appliedEventTypes: undefined,
            appliedOfficeTypes: undefined,
        };

        vm.onSearchFilter = onSearchFilter;
        vm.loadEventsOnPage = loadEventsOnPage;
        vm.viewDetails = viewDetails;
        vm.loadEventsWithNewlyAppliedFilter = loadEventsWithNewlyAppliedFilter;

        init();


        function init() {
            loadEventTypes();
            loadOfficeTypes();
            loadEvents();
        }

        function loadEventTypes() {
            vm.isLoadingControls = true;

            eventRepository.getEventTypes().then(
                function (result) {
                    vm.eventTypes = result;
                    vm.isLoadingControls = false;
                },
                function () {
                    notifySrv.error('errorCodeMessages.messageError');
                }
            );
        }

        function loadEvents() {
            vm.isLoadingEvents = true;

            eventRepository
                .getEventsByTitle(
                    vm.filterText || '',
                    vm.page,
                    vm.filter.appliedEventTypes,
                    vm.filter.appliedOfficeTypes
                )
                .then(
                    function (result) {
                        vm.events = result;
                        vm.isLoadingEvents = false;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
        }

        function loadOfficeTypes() {
            vm.isLoadingControls = true;

            eventRepository.getEventOffices().then(
                function (result) {
                    vm.officeTypes = result;
                    vm.isLoadingControls = false;
                },
                function () {
                    notifySrv.error('errorCodeMessages.messageError');
                }
            );
        }

        function onSearchFilter() {
            loadEventsOnPage(1);
        }

        function loadEventsOnPage(page) {
            vm.page = page;

            loadEvents();
        }

        function viewDetails(id) {
            $state.go('Root.WithOrg.Client.Events.Details.Event', { id: id });
        }

        function loadEventsWithNewlyAppliedFilter(filter, filterName) {
            vm.filter = {
                ...vm.filter,
                [filterName]: filter.map(f => f[0]),
            };

            loadEventsOnPage(1);
        }
    }
})();
