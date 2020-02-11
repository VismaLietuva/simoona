(function () {
    'use strict'

    angular
        .module('simoonaApp.Events')
        .component('aceEventsListDateFilter', {
            templateUrl: 'app/events/list/date-filter/list-date-filter.html',
            controller: eventsListDateFilterController,
            controllerAs: 'vm'
        });

    eventsListDateFilterController.$inject = [
        '$timeout'
    ];

    function eventsListDateFilterController($timeout) {
        var vm = this;

        vm.popoverTemplateUrl = 'app/events/list/date-filter/date-filter-popover.html';
        vm.openDatePicker = openDatePicker;
        vm.isDatePickerOpen = {
            startDate: false,
            endDate: false
        }

        vm.startDate = new Date();
        vm.endDate = new Date();
        vm.endDate.setDate(vm.startDate.getDate() + 7);
        ///

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
    }
})();
