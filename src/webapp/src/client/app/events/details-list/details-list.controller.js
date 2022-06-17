(function () {
    'use strict';
    angular
        .module('simoonaApp.Events')
        .controller('eventDetailsListController', eventDetailsListController);

    eventDetailsListController.$inject = [
        'notifySrv',
        'eventRepository',
        '$state',
        'filterPageTypes',
        'filterTypes',
        'filterPresetRepository',
        'filterPresetService',
    ];

    function eventDetailsListController(
        notifySrv,
        eventRepository,
        $state,
        filterPageTypes,
        filterTypes,
        filterPresetRepository,
        filterPresetService
    ) {
        var vm = this;

        vm.areControlsLoading = true;
        vm.isLoadingEvents = true;
        vm.filterPageType = filterPageTypes.extensiveEventDetailsList;

        vm.page = 1;

        vm.filter = {
            appliedEventTypes: undefined,
            appliedOfficeTypes: undefined,
            sortByColumnName: undefined,
            sortDirection: undefined
        };

        vm.filterTypes = {
            eventTypes: undefined,
            officeTypes: undefined,
        };

        vm.dropdownCheckboxes = {
            eventTypes: new Map(),
            officeTypes: new Map()
        };

        vm.onSearchFilter = onSearchFilter;
        vm.loadEventsOnPage = loadEventsOnPage;
        vm.viewDetails = viewDetails;
        vm.loadEventsWithNewlyAppliedFilter = loadEventsWithNewlyAppliedFilter;
        vm.applyFilterPreset = applyFilterPreset;
        vm.onSort = onSort;

        init();

        function init() {
            loadFilters();
            loadEvents();
        }

        function loadFilters() {
            vm.areControlsLoading = true;

            filterPresetRepository
                .getFilters([filterTypes.events, filterTypes.offices])
                .then(
                    function (result) {
                        vm.filterTypes.eventTypes =
                            filterPresetService.getFiltersByTypeFromResult(
                                result,
                                filterTypes.events
                            );

                        vm.filterTypes.officeTypes =
                            filterPresetService.getFiltersByTypeFromResult(
                                result,
                                filterTypes.offices
                            );

                        vm.areControlsLoading = false;
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
                    vm.filter.appliedOfficeTypes,
                    vm.filter.sortByColumnName,
                    vm.filter.sortDirection
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

        function applyFilterPreset(preset) {
            vm.dropdownCheckboxes.eventTypes = filterPresetService.mapFilterPresetTypesToMap(
                preset,
                filterTypes.events,
                vm.filterTypes
            );

            vm.dropdownCheckboxes.officeTypes = filterPresetService.mapFilterPresetTypesToMap(
                preset,
                filterTypes.offices,
                vm.filterTypes
            );

            applyFilter([...vm.dropdownCheckboxes.eventTypes], 'appliedEventTypes');
            applyFilter([...vm.dropdownCheckboxes.officeTypes], 'appliedOfficeTypes');

            loadEventsOnPage(1);
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

        function applyFilter(filter, filterName) {
            vm.filter[filterName] = filter.map(f => f[0]);
        }

        function loadEventsWithNewlyAppliedFilter(filter, filterName) {
            applyFilter(filter, filterName);
            loadEventsOnPage(1);
        }

        function onSort(sortBy, sortOrder) {
            vm.filter.sortByColumnName = sortBy;
            vm.filter.sortDirection = sortOrder;

            loadEventsOnPage(1);
        }
    }
})();
