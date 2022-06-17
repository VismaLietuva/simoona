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
        vm.visitedEventsPreviewCount = 3;

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
            sortByColumnName: undefined,
            sortDirection: undefined
        };

        vm.showActionsColumn = false;
        vm.page = 1;

        vm.applyFilterPreset = applyFilterPreset;
        vm.setTypes = setTypes;
        vm.loadParticipantsOnPage = loadParticipantsOnPage;
        vm.onSort = onSort;

        init();

        function init() {
            loadFilters();
            loadEventDetails();
            loadParticipants();
        }

        function loadEventDetails() {
            vm.isLoading = true;

            eventRepository
                .getReportEventDetails($stateParams.id)
                .then(function(result) {
                    vm.eventDetails = result;
                    vm.isLoading = false;
                }, function() {
                    notifySrv.error('errorCodeMessages.messageError');
                })
        }

        function loadParticipants() {
            vm.isLoadingParticipants = true;

            eventRepository
                .getEventParticipants(
                    $stateParams.id,
                    vm.filter.appliedKudos,
                    vm.filter.appliedEventTypes,
                    vm.page,
                    vm.filter.sortByColumnName,
                    vm.filter.sortDirection
                )
                .then(
                    function(result) {
                        vm.participants = result;
                        vm.participants.pagedList = vm.participants.pagedList.map(
                            function(element) {
                                var canBeExpanded = vm.visitedEventsPreviewCount < element.visitedEvents.length;

                                if (!vm.showActionsColumn && canBeExpanded) {
                                    vm.showActionsColumn = true;
                                }

                                return {
                                    ...element,
                                    canBeExpanded: canBeExpanded,
                                    isExpanded: false
                                };
                            }
                        );
                        vm.isLoadingParticipants = false;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                )
        }

        function loadFilters() {
            vm.areFiltersLoading = true;

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

                        vm.areFiltersLoading = false;
                    },
                    function() {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                )
        }

        function setTypes(filterName, types) {
            vm.filter[filterName] = types.map(filter => filter[0]);
            loadParticipantsOnPage(1);
        }

        function applyFilterPreset(preset) {
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

            loadParticipantsOnPage(1);
        }

        function loadParticipantsOnPage(page) {
            vm.page = page;
            loadParticipants();
        }

        function onSort(sortBy, sortOrder) {
            vm.filter.sortByColumnName = sortBy;
            vm.filter.sortDirection = sortOrder;

            loadParticipantsOnPage(1);
        }
    }
})();
