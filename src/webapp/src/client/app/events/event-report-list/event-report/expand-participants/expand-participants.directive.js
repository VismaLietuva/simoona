(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .directive('aceExpandParticipants', aceExpandParticipants);

    function aceExpandParticipants() {
        var directive = {
            templateUrl:
                'app/events/event-report-list/event-report/expand-participants/expand-participants.html',
            restrict: 'E',
            replace: true,
            scope: {
                eventTypes: '=',
                userId: '=',
            },
            controller: expandParticipantsController,
            controllerAs: 'vm',
            bindToController: true,
        };

        return directive;
    }

    expandParticipantsController.$inject = [
        '$timeout',
        'eventRepository',
        'notifySrv',
        'sortMultipleLinkService'
    ];

    // TOOD: export to service method that sorting thing?
    function expandParticipantsController($timeout, eventRepository, notifySrv, sortMultipleLinkService) {
        var vm = this;

        var columnCount = 4;

        var maxEndDate = moment().local().startOf('days').toDate();
        var defaultStartDate = moment(maxEndDate).subtract(1, 'year').toDate();

        vm.page = 1;

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

        vm.sortValues = sortMultipleLinkService.getMultipleSort(columnCount);

        vm.loadVisitedEventsOnPage = loadVisitedEventsOnPage;
        vm.loadVisitedEventsWithUpdatedDates = loadVisitedEventsWithUpdatedDates;
        vm.openDatePicker = openDatePicker;
        vm.sortByColumn = sortByColumn;

        init();

        function init() {
            updateDateRestrictions();
            loadVisitedEvents();
        }

        function updateDateRestrictions() {
            vm.datePickers.startDate.options.maxDate = moment.utc(vm.datePickers.endDate.date).local().startOf('day').toDate();
            vm.datePickers.endDate.options.minDate = moment.utc(vm.datePickers.startDate.date).local().startOf('day').toDate();
        }

        function loadVisitedEvents() {
            vm.isLoading = true;

            eventRepository
                .getEventParticipantVisitedEvents(
                    vm.userId,
                    vm.page,
                    vm.sortValues.getSortString(),
                    vm.eventTypes,
                    vm.datePickers.startDate.date,
                    vm.datePickers.endDate.date
                )
                .then(
                    function (response) {
                        vm.isLoading = false;
                        vm.events = response;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
        }

        function sortByColumn(sortBy, sortOrder, position) {
            onCompleteLoadFirstPage(function() {
                vm.sortValues.setSortValues(sortBy, sortOrder, position);
            });
        }

        function onCompleteLoadFirstPage(func) {
            func();
            loadVisitedEventsOnPage(1);
        }

        function loadVisitedEventsWithUpdatedDates() {
            onCompleteLoadFirstPage(function() {
                updateDateRestrictions();
            });
        }

        function loadVisitedEventsOnPage(page) {
            vm.page = page;
            loadVisitedEvents();
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
    }
})();
