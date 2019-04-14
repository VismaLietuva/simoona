(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .controller('EventContentController', EventContentController);

    EventContentController.$inject = [
        '$rootScope',
        '$stateParams',
        '$state',
        '$translate',
        'eventRepository',
        'authService',
        'errorHandler'
    ];

    function EventContentController($rootScope, $stateParams, $state, $translate,
        eventRepository, authService, errorHandler
    ) {
        var vm = this;
        vm.event = {};
        var eventId = $stateParams.id;

        vm.isEventLoading = false;
        vm.currentUser = authService.identity.userId;
        vm.hasAdministrationPermission = authService.hasPermissions(['EVENT_ADMINISTRATION']);

        vm.getEvent = getEvent;
        vm.hasDatePassed = hasDatePassed;

        init();

        /////////

        function init() {
            vm.getEvent();
        }

        function getEvent() {
            vm.isEventLoading = true;
            eventRepository.getEventDetails(eventId).then(function(response) {
                vm.event = response;
                $rootScope.pageTitle = $translate.instant('events.eventTitle') + ' - ' + vm.event.name;
                vm.event.participantsCount = vm.event.participants.length;
                vm.isEventAdmin = vm.hasAdministrationPermission || vm.event.hostUserId === vm.currentUser;
                vm.isEventLoading = false;
            },
            function(error) {
                errorHandler.handleErrorMessage(error);

                $state.go('Root.WithOrg.Client.Events.List.Type', {
                    type: 'all'
                });
            });
        }

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }
    }
})();
