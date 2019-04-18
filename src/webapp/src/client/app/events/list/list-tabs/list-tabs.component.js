(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('defaultEventTabs', [
            { id: 'all' },
            { id: 'participant' },
            { id: 'host' }
        ])
        .component('aceEventsListTabs', {
            replace: true,
            templateUrl: 'app/events/list/list-tabs/list-tabs.html',
            controller: eventsListTabsController,
            controllerAs: 'vm'
        });

    eventsListTabsController.$inject = [
        '$translate',
        '$timeout',
        'eventRepository',
        'defaultEventTabs'
    ];

    function eventsListTabsController($translate, $timeout, eventRepository, defaultEventTabs) {
        /* jshint validthis: true */
        var vm = this;

        vm.eventsTabs = [];
        vm.isLoading = true;

        init();

        ///////////

        function init() {
            eventRepository.getEventTypes().then(function(result) {
                if (result) {
                    vm.eventsTabs = result;
                }

                $timeout(addDefaultTabs, 100);
            });

            eventRepository.getEventOffices().then(function(result) {
                if (result) {
                    vm.eventOffices = result;

                    var allOffices = $translate.instant('events.eventOfficeAll');
                    vm.eventOffices.unshift({ id: "all", name: allOffices });
                }
            });
        }

        function addDefaultTabs() {
            for (var i = 0; i < defaultEventTabs.length; i++) {
                var tabId = defaultEventTabs[i].id;
                if (tabId === 'all') {
                    vm.eventsTabs.unshift({ id: tabId, name: 'events.' + tabId });
                } else {
                    vm.eventsTabs.push({ id: tabId, name: 'events.' + tabId });
                }
            }

            vm.isLoading = false;
        }
    }

})();
