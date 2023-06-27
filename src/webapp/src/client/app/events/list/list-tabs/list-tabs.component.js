(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('defaultEventTabs', [
            { id: 'all' },
            { id: 'main' },
            { id: 'participant' },
            { id: 'host' }
        ])
        .constant('defaultOfficeTabs', [
            { id: 'all' }
        ])
        .component('aceEventsListTabs', {
            replace: true,
            templateUrl: 'app/events/list/list-tabs/list-tabs.html',
            controller: eventsListTabsController,
            controllerAs: 'vm'
        });

    eventsListTabsController.$inject = [
        '$state',
        '$stateParams',
        '$translate',
        '$timeout',
        'eventRepository',
        'defaultEventTabs',
        'defaultOfficeTabs',
        'eventOfficeFactory'
    ];

    function eventsListTabsController($state, $stateParams, $translate, $timeout, eventRepository, defaultEventTabs, defaultOfficeTabs, eventOfficeFactory) {
        /* jshint validthis: true */
        var vm = this;
        vm.eventsTabs = [];
        vm.isLoading = true;

        vm.stateParams = $stateParams;
        vm.onDateChange = onDateChange;
        eventOfficeFactory.getOffices = eventOfficeFactory.getOffices;

        init();

        ///////////

        function init() {
            eventRepository.getEventTypes().then(function(result) {
                if (result) {
                    vm.eventsTabs = result;
                }
                $timeout(addDefaultEventTabs, 100);
            });
            eventOfficeFactory.getOffices().then(function(result) {
                vm.eventOffices = result;
                $timeout(addDefaultOfficeTabs, 100);
            });
        }

        function addDefaultEventTabs() {
            for (var i = 0; i < defaultEventTabs.length; i++) {
                var tabId = defaultEventTabs[i].id;
                if (tabId === 'all' || tabId === 'main') {
                    var translatedTabName = $translate.instant('events.' + tabId + 'Events');
                    vm.eventsTabs.unshift({ id: tabId, name: translatedTabName });
                } else {
                    var translatedType = $translate.instant('events.' + tabId);
                    vm.eventsTabs.push({ id: tabId, name: translatedType });
                }
            }

            vm.isLoading = false;
        }

        function addDefaultOfficeTabs() {
            for (var i = 0; i < defaultOfficeTabs.length; i++) {
                var tabId = defaultOfficeTabs[i].id;
                if (tabId === 'all') {
                    var translatedTabName = $translate.instant('events.eventOfficeAll');
                    vm.eventOffices.unshift({ id: tabId, name: translatedTabName });
                } else {
                    vm.eventOffices.push({ id: tabId, name: tabId });
                }
            }

            vm.isLoading = false;
        }

        function onDateChange(startDate, endDate) {
            $state.go('Root.WithOrg.Client.Events.List.Type', {
                type: $stateParams.type,
                office: $stateParams.office,
                startDate: startDate,
                endDate: endDate
            });
        }
    }
})();
