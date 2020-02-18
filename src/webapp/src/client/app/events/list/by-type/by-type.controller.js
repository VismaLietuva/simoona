(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('eventsSettings', {
            eventsListPageSize: 10
        })
        .controller('eventsByTypeController', eventsByTypeController);

    eventsByTypeController.$inject = [
        '$scope',
        '$stateParams',
        'eventRepository',
        'eventsSettings',
        'eventOfficeFactory',
        'defaultEventTabsNames'
    ];

    function eventsByTypeController($scope, $stateParams, eventRepository, eventsSettings, eventOfficeFactory, defaultEventTabsNames) {
        /*jshint validthis: true */
        var vm = this;

        vm.addMoreEvents = addMoreEvents;

        vm.isEventsFound = true;
        vm.isEventsLoading = false;
        vm.isScrollingEnabled = true;
        vm.eventsList = [];
        vm.eventsListPage = 1;
        vm.itemsDisplayedInList = 0;
        init();

        ///////////

        function init() {}

        function setEventOffices() {
            $scope.$watch(function () {
                    return eventOfficeFactory.offices.data
                },
                function () {
                    if (!eventOfficeFactory.offices.isBusy) {
                        vm.offices = eventOfficeFactory.offices.data;
                        vm.eventsList.forEach(function (event) {
                            if (event.officeIds.length) {
                                mapOfficesNameToEvent(event);
                            }
                        })
                    }
                });
        }

        function mapOfficesNameToEvent(event) {
            event.officesName = [];
            event.officeIds.forEach(function (id) {
                vm.offices.forEach(function (office) {
                    if (id == office.id) {
                        event.officesName.push(office.name);
                    }
                })
            })
        }

        function getMyEvents(typeId, officeId) {
            eventRepository.getMyEvents(typeId, officeId, vm.eventsListPage).then(function (result) {
                result.forEach(elem => vm.eventsList.push(elem));

                isThereMorePages(result.length);
                setEventOffices();
                setResponseUtilities(result);
            });
        }

        function setResponseUtilities(data) {
            vm.isEventsFound = !!data.length;
            vm.isEventsLoading = false;
            vm.itemsDisplayedInList += eventsSettings.eventsListPageSize;
        }

        function addMoreEvents() {
            vm.isEventsLoading = true;

            if ($stateParams.type === defaultEventTabsNames.host || $stateParams.type === defaultEventTabsNames.participant) {
                getMyEvents($stateParams.type, $stateParams.office);
            } else {
                let params = {
                    officeId: $stateParams.office,
                    typeId: $stateParams.type,
                    startDate: $stateParams.startDate,
                    endDate: $stateParams.endDate,
                    page: vm.eventsListPage
                }

                eventRepository.getEventsByTypeAndOffice(params).then(function (result) {
                    result.forEach(elem => vm.eventsList.push(elem));

                    isThereMorePages(result.length);
                    setEventOffices();
                    setResponseUtilities(result);
                });
            }
        }

        function isThereMorePages(resultLength) {
            if (resultLength === eventsSettings.eventsListPageSize) {
                vm.eventsListPage++;
            } else {
                vm.isScrollingEnabled = false;
            }
        }
    }
})();
