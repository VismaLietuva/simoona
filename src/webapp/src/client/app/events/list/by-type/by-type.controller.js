(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('eventsSettings', {
            eventsListPageSize: 20
        })
        .controller('eventsByTypeController', eventsByTypeController);

    eventsByTypeController.$inject = [
        '$scope',
        '$stateParams',
        'eventRepository',
        'eventsSettings',
        'eventOfficeFactory'
    ];

    function eventsByTypeController($scope,$stateParams, eventRepository, eventsSettings, eventOfficeFactory) {
        /*jshint validthis: true */
        var vm = this;

        vm.addMoreEvents = addMoreEvents;

        $scope.$watch(function() {return eventOfficeFactory.offices.data},
         function() {
             if(!eventOfficeFactory.offices.isBusy) 
             {
                 vm.offices = eventOfficeFactory.offices.data;
                 vm.eventsList.forEach(function(event) {
                    if (event.officeIds.length) {
                        mapOfficesNameToEvent(event);
                    }         
                })
             }
            });
        
        vm.isEventsFound = true;
        vm.isEventsLoading = true;
        vm.eventsList = [];
        vm.itemsDisplayedInList = eventsSettings.eventsListPageSize;
        init();

        ///////////

        function init() {
            if ($stateParams.type === 'all' && $stateParams.office === 'all') {
                eventRepository.getAllEvents().then(function (result) {
                    vm.eventsList = result;
                    setResponseUtilities(result);
                });
            } else if ($stateParams.type === 'host' || $stateParams.type === 'participant') {
                getMyEvents($stateParams.type, $stateParams.office);
            } else {
                eventRepository.getEventsByTypeAndOffice($stateParams.type, $stateParams.office).then(function (result) {
                    vm.eventsList = result;
                    vm.eventsList.forEach(function(event) {
                        if (event.officeIds.length) {
                            mapOfficesNameToEvent(event);
                        }         
                    })
                    setResponseUtilities(result);
                });
            }
            
        }

        function mapOfficesNameToEvent(event) {
            event.officesName = [];
            event.officeIds.forEach(function(id) {
                vm.offices.forEach(function(office) {
                    if(id == office.id)
                    {
                        event.officesName.push(office.name);                              
                    }
            })
        })
        }

        function getMyEvents(typeId, officeId) {
            eventRepository.getMyEvents(typeId, officeId).then(function (result) {
                vm.eventsList = result;
                vm.eventsList.forEach(function(event) {
                        if (event.officeIds.length) {
                            mapOfficesNameToEvent(event);
                        }         
                    })
                setResponseUtilities(result);
            });
        }

        function setResponseUtilities(data) {
            vm.isEventsFound = !!data.length;
            vm.isEventsLoading = false;
        }

        function addMoreEvents() {
            if (vm.itemsDisplayedInList < vm.eventsList.length) {
                vm.itemsDisplayedInList = vm.itemsDisplayedInList + eventsSettings.eventsListPageSize;
            }
        }
    }
})();
