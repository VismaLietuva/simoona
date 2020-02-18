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
            controllerAs: 'vm'
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
            if (difference > customRangeOptions.allowedDifference) {
                notifySrv.error('events.dateFilterMaxRangeError');
                return;
            }

            var options = getSelectedOptions();

            $state.go('Root.WithOrg.Client.Events.List.Type', {
                type: $stateParams.type,
                office: $stateParams.office,
                startDate: options.startDate,
                endDate: options.endDate
            });

            vm.buttonTitle = options.title;
            vm.isPopoverOpen = false;
        }

        function clearFilter() {
            if ($stateParams.endDate || $stateParams.endDate) {
                $state.go('Root.WithOrg.Client.Events.List.Type', {
                    type: $stateParams.type,
                    office: $stateParams.office,
                    startDate: null,
                    endDate: null
                });

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
                    options.startDate = vm.dateFilterStart;
                    options.endDate = vm.dateFilterEnd;
                    options.title = $translate.instant('events.customRange');
                    break;
            }

            return options;
        }

        function isDateFilterSelected() {
            return $stateParams.startDate && $stateParams.endDate;
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
    }
})();
