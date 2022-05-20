(function () {
    'use strict';
    angular
        .module('simoonaApp.Events')
        .controller('eventDetailsListController', eventDetailsListController);

    eventDetailsListController.$inject = [
        'authService',
        'notifySrv',
        'eventRepository',
        '$state',
    ];

    function eventDetailsListController(
        authService,
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

        vm.onListFilter = onListFilter;
        vm.changePage = changePage;
        vm.viewDetails = viewDetails;
        vm.eventTypesChange = eventTypesChange;
        vm.officeTypesChange = officeTypesChange;

        loadEventTypes();
        loadOfficeTypes();
        loadEvents();

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

        function onListFilter() {
            vm.page = 1;
            loadEvents();
        }

        function changePage(page) {
            vm.page = page;
            loadEvents();
        }

        function viewDetails(id) {
            $state.go('Root.WithOrg.Client.Events.Details.Event', { id: id });
        }

        function eventTypesChange(appliedEventTypes) {
            loadEventsWithFilter(appliedEventTypes, 'appliedEventTypes');
        }

        function officeTypesChange(appliedOfficeTypes) {
            loadEventsWithFilter(appliedOfficeTypes, 'appliedOfficeTypes');
        }

        function loadEventsWithFilter(filter, filterName) {
            vm.page = 1;

            vm.filter = {
                ...vm.filter,
                [filterName]: filter,
            };

            loadEvents();
        }
    }
})();
