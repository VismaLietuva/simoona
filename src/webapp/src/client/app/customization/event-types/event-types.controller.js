(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.EventTypes')
        .controller('eventTypesController', eventTypesController);

    eventTypesController.$inject = [
        '$rootScope',
        'eventTypesRepository',
        'errorHandler'
    ];

    function eventTypesController($rootScope, eventTypesRepository, errorHandler) {
        var vm = this;

        $rootScope.pageTitle = 'customization.eventTypes';
        
        vm.isLoading = true;
        vm.eventTypes = [];

        init();

        ///////////

        function init() {
            eventTypesRepository.getEventTypes().then(function(response) {
                vm.eventTypes = response;
                vm.isLoading = false;
            }, errorHandler.handleErrorMessage);
        }
    }
})();
