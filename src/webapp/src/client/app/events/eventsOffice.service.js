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
            isBusy: true
        };

        var service = {
            offices: offices,

            getOffices: getOffices,
        };

        return service;


        ////////

        function getOffices() {
            offices.isBusy = true;
            return eventRepository.getEventOffices().then(function (response){
                offices.data = response;
                offices.isBusy = false;
                return response;
            }, errorHandler.handleErrorMessage);
        }
        
        
    }
})();
