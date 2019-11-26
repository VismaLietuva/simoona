(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('eventOfficeFactory', eventOfficeFactory);

    eventOfficeFactory.$inject = [
        'eventRepository',
        'errorHandler'
    ];

    function eventOfficeFactory(eventRepository, errorHandler) {
        
        var offices = {
            data: [],
            isBusy: false
        };

        var service = {
            offices: offices,

            getOffices: getOffices,
        };

        getOffices();

        return service;


        ////////

        function getOffices() {
            eventRepository.getEventOffices().then(function (response){
                offices.data = response;
            }, errorHandler.handleErrorMessage);
        }
        
        
    }
})();
