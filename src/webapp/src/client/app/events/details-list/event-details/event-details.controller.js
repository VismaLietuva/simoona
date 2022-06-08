(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .controller('eventDetailsController', eventDetailsController);

    eventDetailsController.$inject = [
        'eventRepository',
        '$stateParams',
        'eventSettings',
        'notifySrv',
        'filterPresetRepository',
        'filterPresetService',
        'filterTypes',
        'filterPageTypes'
    ];

    function eventDetailsController(
        eventRepository,
        $stateParams,
        eventSettings,
        notifySrv,
        filterPresetRepository,
        filterPresetService,
        filterTypes,
        filterPageTypes
    ) {
        var vm = this;

        vm.filterPageType = filterPageTypes.extensiveEventDetails;

        vm.eventImageSize = {
            w: eventSettings.thumbWidth,
            h: eventSettings.thumbHeight,
        };

        vm.filterTypes = {
            eventTypes: undefined,
            kudosTypes: undefined
        }

        vm.dropdownCheckboxes = {
            eventTypes: new Map(),
            kudosTypes: new Map()
        }

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
        vm.applyFilterPreset = applyFilterPreset;

        init();

        function init() {
            loadFilters();
            loadEventDetails();
        }

        function loadFilters() {
            vm.isLoading = true;

            filterPresetRepository
                .getFilters([filterTypes.events, filterTypes.kudos])
                .then(
                    function(result) {
                        vm.filterTypes.eventTypes =
                            filterPresetService.getFiltersByTypeFromResult(
                                result,
                                filterTypes.events
                            );

                        vm.filterTypes.kudosTypes =
                            filterPresetService.getFiltersByTypeFromResult(
                                result,
                                filterTypes.kudos
                            );

                        if (vm.notAppliedPreset !== undefined) {
                            handleNotAppliedPreset();
                        }

                        vm.isLoading = false;
                    },
                    function() {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                )
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

        function applyFilterPreset(preset) {
            if (!vm.filterTypes.eventTypes || !vm.filterTypes.kudosTypes) {
                vm.notAppliedPreset = preset;
                return;
            }

            vm.dropdownCheckboxes.eventTypes = filterPresetService.mapFilterPresetTypesToMap(
                preset,
                filterTypes.events,
                vm.filterTypes
            );

            vm.dropdownCheckboxes.kudosTypes = filterPresetService.mapFilterPresetTypesToMap(
                preset,
                filterTypes.kudos,
                vm.filterTypes
            );

            vm.filter.appliedEventTypes = [...vm.dropdownCheckboxes.eventTypes.keys()];
            vm.filter.appliedKudos = [...vm.dropdownCheckboxes.kudosTypes.keys()];

            loadEventDetails();
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

        function handleNotAppliedPreset() {
            vm.applyFilterPreset(vm.notAppliedPreset);
            vm.notAppliedPreset = undefined;
        }

        function showActionsColumn() {
            return (
                vm.pageContent &&
                vm.pageContent.find((participant) => participant.canBeExpanded)
            );
        }
    }
})();
