(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventDescription', {
            bindings: {
                event: '=',
                isLoading: '='
            },
            templateUrl: 'app/events/content/description/description.html',
            controller: eventDescriptionController,
            controllerAs: 'vm'
        });

    eventDescriptionController.$inject = [
        'eventSettings'
    ];

    function eventDescriptionController(eventSettings) {
        /* jshint validthis: true */
        var vm = this;

        vm.eventImageSize = {
            w: eventSettings.thumbWidth,
            h: eventSettings.thumbHeight
        };

        vm.hasDatePassed = hasDatePassed;
        
        ////////

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }
    }
})();
