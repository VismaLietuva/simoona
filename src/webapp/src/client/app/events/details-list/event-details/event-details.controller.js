(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .controller('eventDetailsController', eventDetailsController);

    eventDetailsController.$inject = [
        'eventRepository',
        'kudosTypesRepository',
        '$stateParams',
        'eventSettings',
        'notifySrv',
    ];

    function eventDetailsController(
        eventRepository,
        kudosTypesRepository,
        $stateParams,
        eventSettings,
        notifySrv
    ) {
        var vm = this;

        vm.eventImageSize = {
            w: eventSettings.thumbWidth,
            h: eventSettings.thumbHeight,
        };

        vm.filter = {
            appliedKudos: undefined,
            appliedEventTypes: undefined,
        };

        vm.settings = {
            showItemsInEventsList: 3,
            pageSize: 10
        };

        vm.page = 1;

        vm.setKudosTypes = setKudosTypes;
        vm.setEventTypes = setEventTypes;
        vm.changePage = changePage;

        init();

        function init() {
            loadKudosTypes();
            loadEventTypes();
            loadEventDetails();
        }

        function loadEventDetails() {
            vm.isLoading = true;
            vm.pageContent = undefined;
            eventRepository
                .getExtensiveEventDetails(
                    $stateParams.id,
                    vm.filter.appliedKudos,
                    vm.filter.appliedEventTypes
                )
                .then(
                    function (result) {
                        vm.eventDetails = result;
                        changePage(1);
                        vm.isLoading = false;
                        vm.loadControls =
                            vm.eventDetails.extensiveParticipants.length;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
        }

        function loadKudosTypes() {
            kudosTypesRepository.getKudosTypes().then(
                function (result) {
                    vm.kudosTypes = result;
                },
                function () {
                    notifySrv.error('errorCodeMessages.messageError');
                }
            );
        }

        function loadEventTypes() {
            eventRepository.getEventTypes().then(
                function (result) {
                    vm.eventTypes = result;
                },
                function () {
                    notifySrv.error('errorCodeMessages.messageError');
                }
            );
        }

        function setKudosTypes(types) {
            vm.filter.appliedKudos = types.map((filter) => filter[1]);

            loadEventDetails();
        }

        function setEventTypes(types) {
            vm.filter.appliedEventTypes = types.map((filter) => filter[0]);

            loadEventDetails();
        }

        function changePage(page) {
            vm.page = page;

            loadPage();
        }

        function loadPage() {
            var start = (vm.page - 1) * vm.settings.pageSize;
            var offset = vm.settings.pageSize;

            vm.pageContent = vm.eventDetails.extensiveParticipants
                .slice(
                    start,
                    start + offset
                )
                .map((participant) => ({
                    ...participant,
                    isExpanded: false,
                    canBeExpanded:
                        participant.visitedEvents.length >
                        vm.settings.showItemsInEventsList
                }));

            vm.showActionsColumn = showActionsColumn();
        }

        function showActionsColumn() {
            return (
                vm.pageContent &&
                vm.pageContent.find((participant) => participant.canBeExpanded)
            );
        }
    }
})();
