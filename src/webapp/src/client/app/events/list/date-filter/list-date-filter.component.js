(function () {
    'use strict'

    angular
        .module('simoonaApp.Events')
        .constant('dateRanges', {
            none: 0,
            pastWeek: 1,
            pastMonth: 2,
            pastYear: 3,
            custom: 4
        })
        .component('aceEventsListDateFilter', {
            templateUrl: 'app/events/list/date-filter/list-date-filter.html',
            controller: eventsListDateFilterController,
            controllerAs: 'vm'
        });

    eventsListDateFilterController.$inject = [
        '$timeout',
        '$state',
        '$stateParams',
        'dateRanges',
        '$translate'
    ];

    function eventsListDateFilterController($timeout, $state, $stateParams, dateRanges, $translate) {
        var vm = this;

        vm.popoverTemplateUrl = 'app/events/list/date-filter/date-filter-popover.html';
        vm.isDateFilterSelected = isDateFilterSelected;
        vm.openDatePicker = openDatePicker;
        vm.getFilteredEvents = getFilteredEvents;
        vm.buttonTitle = $translate.instant('events.selectDate');

        vm.dateRanges = dateRanges;
        vm.selectedRange = vm.dateRanges.none;


        init();

        ///////////

        function init() {
            vm.isDatePickerOpen = {
                startDate: false,
                endDate: false
            }
        }

        function getFilteredEvents() {
            var options = getSelectedDates();

            $state.go('Root.WithOrg.Client.Events.List.Type', {
                type: $stateParams.type,
                office: $stateParams.office,
                startDate: options.startDate,
                endDate: options.endDate
            });

            vm.buttonTitle = options.title;
        }

        function getSelectedDates() {
            var options = {
                startDate: new Date(),
                endDate: new Date(),
                title: ''
            }

            switch (vm.selectedRange) {
                case vm.dateRanges.pastWeek:
                    options.startDate.setDate(options.startDate.getDate() - 7);
                    options.title = $translate.instant('events.pastWeek');
                    break;
                case vm.dateRanges.pastMonth:
                    options.startDate.setMonth(options.startDate.getMonth() - 1);
                    options.title = $translate.instant('events.pastMonth');
                    break;
                case vm.dateRanges.pastYear:
                    options.startDate.setFullYear(options.startDate.getFullYear() - 1);
                    options.title = $translate.instant('events.pastYear');
                    break;
                case vm.dateRanges.custom:
                    options.startDate = vm.dateFilterStart;
                    options.endDate = vm.dateFilterEnd;
                    options.title = $translate.instant('events.customRange');
                    break;
            }

            return options;
        }

        function openDatePicker($event, datepicker) {
            $event.preventDefault();
            $event.stopPropagation();

            vm.isDatePickerOpen.startDate = false;
            vm.isDatePickerOpen.endDate = false;
            vm.isDatePickerOpen[datepicker] = true;

            $timeout(function () {
                $event.target.focus();
            }, 100);
        }

        function isDateFilterSelected() {
            return $stateParams.startDate && $stateParams.endDate;
        }
    }
})();
