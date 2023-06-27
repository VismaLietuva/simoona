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

        var tableColumnCount = 6;

        vm.filter = eventReportService.getEventReportFilter(
            filterPageTypes.eventReportList, [
                { name: 'events', type: filterTypes.events },
                { name: 'offices', type: filterTypes.offices }
            ],
            tableColumnCount
        );

        vm.page = 1;
        vm.startDate = null;
        vm.endDate = null;

        vm.loadEventsOnPage = loadEventsOnPage;
        vm.viewDetails = viewDetails;
        vm.loadEventsWithNewlyAppliedFilter = loadEventsWithNewlyAppliedFilter;
        vm.applyFilterPreset = applyFilterPreset;
        vm.sortByColumn = sortByColumn;
        vm.onDateFilterChange = onDateFilterChange;

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
                    vm.filter.getSortString(),
                    vm.startDate,
                    vm.endDate,
                    vm.excludeEmptyEvents
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

        function onDateFilterChange(startDate, endDate){
            vm.startDate = startDate;
            vm.endDate = endDate;

            loadEventsOnPage(1);
        }

        function sortByColumn(sortBy, sortOrder, position) {
            onCompleteLoadFirstPage(function () {
                vm.filter.setSortValues(sortBy, sortOrder, position);
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
