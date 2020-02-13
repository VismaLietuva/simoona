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
        '$state',
        '$stateParams',
        'dateRanges',
        '$translate'
    ];

    function eventsListDateFilterController($state, $stateParams, dateRanges, $translate) {
        var vm = this;

        vm.popoverTemplateUrl = 'app/events/list/date-filter/date-filter-popover.html';
        vm.buttonTitle = $translate.instant('events.selectDate');
        vm.isPopoverOpen = false;

        vm.getFilteredEvents = getFilteredEvents;
        vm.clearFilter = clearFilter;
        vm.openDatePicker = openDatePicker;
        vm.isDateFilterSelected = isDateFilterSelected;
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

        function isDateFilterSelected() {
            return $stateParams.startDate && $stateParams.endDate;
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
    }
})();
