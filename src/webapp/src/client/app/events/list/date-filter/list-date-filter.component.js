(function () {
    'use strict'

    angular
        .module('simoonaApp.Events')
        .constant('dateRanges', {
            none: 0,
            pastWeek: 1,
            pastMonth: 2,
            pastThreeMonths: 3,
            custom: 4
        })
        .constant('customRangeOptions', {
            allowedDifference: 100
        })
        .component('aceEventsListDateFilter', {
            templateUrl: 'app/events/list/date-filter/list-date-filter.html',
            controller: eventsListDateFilterController,
            controllerAs: 'vm',
            bindings: {
                onChange: '&',
            }
        });

    eventsListDateFilterController.$inject = [
        '$state',
        '$stateParams',
        'dateRanges',
        '$translate',
        'defaultEventTabsNames',
        'customRangeOptions',
        'notifySrv'
    ];

    function eventsListDateFilterController($state, $stateParams, dateRanges, $translate, defaultEventTabsNames, customRangeOptions, notifySrv) {
        var vm = this;

        vm.popoverTemplateUrl = 'app/events/list/date-filter/date-filter-popover.html';
        vm.buttonTitle = $translate.instant('events.selectDate');
        vm.isPopoverOpen = false;
        vm.getFilteredEvents = getFilteredEvents;
        vm.clearFilter = clearFilter;
        vm.openDatePicker = openDatePicker;
        vm.isDateFilterSelected = isDateFilterSelected;
        vm.isDateFilterDisabled = isDateFilterDisabled;
        vm.isCustomRangeInvalid = isCustomRangeInvalid;

        vm.dateRanges = dateRanges;

        vm.startDate = null;
        vm.endDate = null;

        init();

        ///////////

        function init() {
            vm.isDatePickerOpen = {
                startDate: false,
                endDate: false
            }
            vm.selectedRange = vm.dateRanges.none;

        }

        function getFilteredEvents() {
            let difference = dateDifferenceInDays(vm.dateFilterStart, vm.dateFilterEnd)
            if (difference > customRangeOptions.allowedDifference && vm.selectedRange === vm.dateRanges.custom) {
                notifySrv.error('events.dateFilterMaxRangeError');
                return;
            }

            var options = getSelectedOptions();

            vm.startDate = options.startDate;
            vm.endDate = options.endDate;

            vm.onChange({startDate: vm.startDate, endDate: vm.endDate});

            vm.buttonTitle = options.title;
            vm.isPopoverOpen = false;
        }

        function clearFilter() {
            if (vm.startDate || vm.endDate) {
                vm.startDate = null;
                vm.endDate = null;

                vm.onChange({startDate: null, endDate: null});

                vm.buttonTitle = $translate.instant('events.selectDate');
                vm.selectedRange = vm.dateRanges.none;
                vm.isPopoverOpen = false;
            }
        }

        function getSelectedOptions() {
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
                case vm.dateRanges.pastThreeMonths:
                    options.startDate.setMonth(options.startDate.getMonth() - 3);
                    options.title = $translate.instant('events.pastThreeMonths');
                    break;
                case vm.dateRanges.custom:
                    let startDate = vm.dateFilterStart;
                    let endDate = vm.dateFilterEnd;
                    if (startDate && endDate && startDate.getTime() === endDate.getTime()) {
                        // To get range from 0:00 to 23:59
                        endDate = new Date(endDate.setDate(endDate.getDate() + 1));
                    }

                    options.startDate = toISOStringWithoutTime(startDate);
                    options.endDate = toISOStringWithoutTime(endDate);

                    options.title = $translate.instant('events.customRange');
                    break;
            }

            return options;
        }

        function isDateFilterSelected() {
            return vm.startDate && vm.endDate;
        }

        function isDateFilterDisabled() {
            if ($stateParams.type == defaultEventTabsNames.host ||
                $stateParams.type == defaultEventTabsNames.participant) {
                clearFilter();
                return true;
            }
            return false;
        }

        function openDatePicker($event, datepicker) {
            $event.preventDefault();
            $event.stopPropagation();

            vm.isDatePickerOpen.startDate = false;
            vm.isDatePickerOpen.endDate = false;
            vm.isDatePickerOpen[datepicker] = true;
        }

        function isCustomRangeInvalid() {
            return vm.dateFilterStart > vm.dateFilterEnd &&
                vm.selectedRange === vm.dateRanges.custom;
        }

        function dateDifferenceInDays(start, end) {
            const MS_PER_DAY = 1000 * 60 * 60 * 24;
            return (end - start) / MS_PER_DAY;
        }

        function toISOStringWithoutTime(date) {
            return date.getFullYear() + '-' + pad(date.getMonth() + 1) + '-' + pad(date.getDate());
        };

        function pad(number) {
            if (number < 10) {
              return '0' + number;
            }
            return number;
        }
    }
})();
