(function () {
    'use strict';
    angular
        .module('simoonaApp.Events')
        .controller('EventDetailsListController', EventDetailsListController);

    EventDetailsListController.$inject = [
        'authService',
        'notifySrv',
        'eventRepository',
        '$state'
    ];

    function EventDetailsListController(authService, notifySrv, eventRepository, $state) {
        var vm = this;

        vm.isLoadingControls = true;
        vm.isLoadingEvents = true;

        vm.page = 1;

        vm.onListFilter = onListFilter;
        vm.setFilter = setFilter;
        vm.changePage = changePage;
        vm.viewDetails = viewDetails;
        vm.eventTypesChange = eventTypesChange;

        loadEventTypes();
        loadEvents();


        function loadEventTypes() {
            eventRepository.getEventTypes().then(function (result) {
                vm.eventTypes = result;
                vm.isLoadingControls = false;
            }, function () {
                notifySrv.error('errorCodeMessages.messageError');
            })
        }

        function loadEvents(searchString = "", page = 1, typeId = undefined) {
            vm.isLoadingEvents = true;
            eventRepository.getEventsByTitle(searchString, page, typeId).then(function (result) {
                vm.events = result;

                vm.isLoadingEvents = false;
            }, function () {
                notifySrv.error('errorCodeMessages.messageError');
            });
        }

        function onListFilter() {
            loadEvents(vm.filterText);
        }

        function setFilter(typeId) {
            vm.filter = typeId;
            vm.filterText = "";
            vm.page = 1;

            loadEvents(vm.filterText, vm.page, typeId);
        }

        function changePage(page) {
            vm.page = page;
            loadEvents(vm.filterText, vm.page, vm.filter)
        }

        function viewDetails(id) {
            $state.go('Root.WithOrg.Client.Events.Details.Event', { id: id });
        }

        function eventTypesChange(appliedTypes) {
            console.log(appliedTypes);
        }
    }
})();
