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
        '$timeout',
        '$state',
        '$stateParams'
    ];

    function eventsListDateFilterController($timeout, $state, $stateParams) {
        var vm = this;

        vm.popoverTemplateUrl = 'app/events/list/date-filter/date-filter-popover.html';
        vm.openDatePicker = openDatePicker;
        vm.getFilteredEvents = getFilteredEvents;

        init();

        ///////////

        function init() {
            vm.dateFilterStart = new Date(moment.utc().subtract(7, 'd').format('LL'));
            vm.dateFilterEnd = new Date(moment.utc().format('LL'));

            vm.isDatePickerOpen = {
                startDate: false,
                endDate: false
            }
        }

        function getFilteredEvents() {
            $state.go('Root.WithOrg.Client.Events.List.Type', {
                type: $stateParams.type,
                office: $stateParams.office,
                startDate: vm.dateFilterStart,
                endDate: vm.dateFilterEnd
            });
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
    }
})();
