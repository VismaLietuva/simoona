(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('visitedEventsPreviewCount', 3)
        .controller('eventReportController', eventReportController);

    eventReportController.$inject = [
        'eventRepository',
        '$stateParams',
        'eventSettings',
        'notifySrv',
        'filterPresetRepository',
        'filterTypes',
        'filterPageTypes',
        'eventReportService',
        'visitedEventsPreviewCount',
    ];

    function eventReportController(
        eventRepository,
        $stateParams,
        eventSettings,
        notifySrv,
        filterPresetRepository,
        filterTypes,
        filterPageTypes,
        eventReportService,
        visitedEventsPreviewCount
    ) {
        var vm = this;

        vm.isLoading = {
            events: true,
            participants: true,
            filters: true,
        };

        var tableColumnCount = 8;

        vm.filter = eventReportService.getEventReportFilter(
            filterPageTypes.eventReport, [
                { name: 'events', type: filterTypes.events },
                { name: 'kudos', type: filterTypes.kudos }
            ],
            tableColumnCount
        );

        vm.visitedEventsPreviewCount = visitedEventsPreviewCount;

        vm.eventImageSize = {
            w: eventSettings.thumbWidth,
            h: eventSettings.thumbHeight,
        };

        vm.showActionsColumn = false;
        vm.page = 1;

        vm.applyFilterPreset = applyFilterPreset;
        vm.loadParticipantsWithNewlyAppliedFilter = loadParticipantsWithNewlyAppliedFilter;
        vm.loadParticipantsOnPage = loadParticipantsOnPage;
        vm.sortByColumn = sortByColumn;

        init();

        function init() {
            loadFilters();
            loadEventDetails();
        }

        function loadEventDetails() {
            vm.isLoading.events = true;

            eventRepository.getReportEventDetails($stateParams.id).then(
                function (result) {
                    vm.eventDetails = result;
                    vm.isLoading.events = false;
                },
                function () {
                    notifySrv.error('errorCodeMessages.messageError');
                }
            );
        }

        function loadParticipants() {
            vm.isLoading.participants = true;
            vm.showActionsColumn = false;

            eventRepository
                .getEventParticipants(
                    $stateParams.id,
                    vm.filter.appliedFilters.kudos,
                    vm.filter.appliedFilters.events,
                    vm.page,
                    vm.filter.getSortString()
                )
                .then(
                    function (result) {
                        vm.participants = result;
                        vm.participants.pagedList =
                            vm.participants.pagedList.map(function (element) {
                                var canBeExpanded =
                                    vm.visitedEventsPreviewCount <
                                    element.visitedEvents.length;

                                if (!vm.showActionsColumn && canBeExpanded) {
                                    vm.showActionsColumn = true;
                                }

                                return Object.assign({
                                    canBeExpanded: canBeExpanded,
                                    isExpanded: false,
                                }, element);
                            });

                        vm.isLoading.participants = false;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
        }

        function loadFilters() {
            vm.isLoading.filters = true;

            filterPresetRepository
                .getFilters([filterTypes.events, filterTypes.kudos])
                .then(
                    function (result) {
                        vm.filter.setFilterTypes(result);
                        vm.isLoading.filters = false;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
        }

        function loadParticipantsWithNewlyAppliedFilter(filter, filterName) {
            onCompleteLoadFirstPage(function () {
                vm.filter.updateAppliedFilter(filter, filterName);
            });
        }

        // This function is called after aceFilterPreset is initialized
        function applyFilterPreset(preset) {
            onCompleteLoadFirstPage(function () {
                if (vm.currentPreset === preset) {
                    return;
                }

                if (!preset) {
                    vm.currentPreset = preset;

                    loadParticipantsOnPage(1);
                    return;
                }

                vm.filter.updateAppliedFilters(preset);
            });
        }

        function sortByColumn(sortBy, sortOrder, position) {
            onCompleteLoadFirstPage(function () {
                vm.filter.setSortValues(sortBy, sortOrder, position);
            });
        }

        function onCompleteLoadFirstPage(func) {
            func();
            loadParticipantsOnPage(1);
        }

        function loadParticipantsOnPage(page) {
            vm.page = page;
            loadParticipants();
        }
    }
})();
