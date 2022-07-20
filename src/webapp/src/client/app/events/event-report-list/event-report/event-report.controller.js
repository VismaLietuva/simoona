(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('visitedEventsPreviewCount', 3)
        .controller('eventReportController', eventReportController);

    eventReportController.$inject = [
        'eventRepository',
        '$timeout',
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
        $timeout,
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
        var maxEndDate = moment().local().startOf('days').toDate();
        var defaultStartDate = moment(maxEndDate).subtract(2, 'year').toDate();

        vm.datePickers = {
            startDate: {
                isOpen: false,
                date: defaultStartDate,
                options: {
                    startingDay: 1,
                    datepickerMode: 'year',
                    maxDate: moment(maxEndDate).subtract(1, 'day').toDate()
                }
            },
            endDate: {
                isOpen: false,
                date: maxEndDate,
                options: {
                    startingDay: 1,
                    datepickerMode: 'year',
                    maxDate: maxEndDate,
                    minDate: defaultStartDate
                }
            }
        }

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
        vm.openDatePicker = openDatePicker;
        vm.loadVisitedEventsWithUpdatedDates = loadVisitedEventsWithUpdatedDates;

        init();

        function init() {
            updateDateRestrictions();
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
                    vm.filter.getSortString(),
                    vm.datePickers.startDate.date,
                    vm.datePickers.endDate.date
                )
                .then(
                    function (result) {
                        vm.participants = result;
                        vm.participants.pagedList =
                            vm.participants.pagedList.map(function (element) {
                                var canBeExpanded =
                                    vm.visitedEventsPreviewCount <
                                    element.totalVisitedEventCount;

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

        function openDatePicker($event, key) {
            $event.preventDefault();
            $event.stopPropagation();

            closeAllDatePickers(key);

            vm.datePickers[key].isOpen = true;

            $timeout(function() {
                $event.target.focus();
            }, 100);
        }

        function closeAllDatePickers() {
            vm.datePickers.startDate.isOpen = false;
            vm.datePickers.endDate.isOpen = false;
        }

        function updateDateRestrictions() {
            vm.datePickers.startDate.options.maxDate = moment.utc(vm.datePickers.endDate.date).local().startOf('day').toDate();
            vm.datePickers.endDate.options.minDate = moment.utc(vm.datePickers.startDate.date).local().startOf('day').toDate();
        }

        function loadVisitedEventsWithUpdatedDates() {
            onCompleteLoadFirstPage(function() {
                updateDateRestrictions();
            });
        }
    }
})();
