(function () {
    'use strict';
    angular
        .module('simoonaApp.Events')
        .controller('eventReportListController', eventReportListController);

    eventReportListController.$inject = [
        'notifySrv',
        'eventRepository',
        '$state',
        'filterPageTypes',
        'filterTypes',
        'filterPresetRepository',
        'eventReportService'
    ];

    function eventReportListController(
        notifySrv,
        eventRepository,
        $state,
        filterPageTypes,
        filterTypes,
        filterPresetRepository,
        eventReportService
    ) {
        var vm = this;

        vm.isLoading = {
            filters: true,
            events: true
        };

        vm.filter = eventReportService.getEventReportFilter(
            filterPageTypes.eventReportList, [
                { name: 'events', type: filterTypes.events },
                { name: 'offices', type: filterTypes.offices }
            ]
        );

        vm.page = 1;

        vm.loadEventsOnPage = loadEventsOnPage;
        vm.viewDetails = viewDetails;
        vm.loadEventsWithNewlyAppliedFilter = loadEventsWithNewlyAppliedFilter;
        vm.applyFilterPreset = applyFilterPreset;
        vm.sortByColumn = sortByColumn;

        init();


        function init() {
            loadFilters();
        }

        function loadFilters() {
            vm.isLoading.filters = true;

            filterPresetRepository
                .getFilters([filterTypes.events, filterTypes.offices])
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

        function loadEvents() {
            vm.isLoading.events = true;

            eventRepository
                .getEventsByTitle(
                    vm.filterText || '',
                    vm.page,
                    vm.filter.appliedFilters.events,
                    vm.filter.appliedFilters.offices,
                    vm.filter.appliedFilters.sortBy,
                    vm.filter.appliedFilters.sortOrder
                )
                .then(
                    function (result) {
                        vm.events = result;
                        vm.isLoading.events = false;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
        }

        // This function is called after aceFilterPreset is initialized
        function applyFilterPreset(preset) {
            onCompleteLoadFirstPage(function() {
                if (vm.currentPreset === preset) {
                    return;
                }

                if (!preset) {
                    vm.currentPreset = preset;

                    loadEventsOnPage(1);

                    return;
                }

                vm.filter.updateAppliedFilters(preset);
            });
        }

        function viewDetails(id) {
            $state.go('Root.WithOrg.Client.Events.Report.Event', { id: id });
        }

        function loadEventsWithNewlyAppliedFilter(filter, filterName) {
            onCompleteLoadFirstPage(function() {
                vm.filter.updateAppliedFilter(filter, filterName);
            });
        }

        function sortByColumn(sortBy, sortOrder) {
            onCompleteLoadFirstPage(function () {
                vm.filter.setSortValues(sortBy, sortOrder);
            });
        }

        function onCompleteLoadFirstPage(func) {
            func();
            loadEventsOnPage(1);
        }

        function loadEventsOnPage(page) {
            vm.page = page;

            loadEvents();
        }
    }
})();
