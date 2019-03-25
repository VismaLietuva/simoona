(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventDescriptionActions', {
            bindings: {
                event: '='
            },
            templateUrl: 'app/events/content/description/description-actions/description-actions.html',
            controller: eventDescriptionActionsController,
            controllerAs: 'vm'
        });

    eventDescriptionActionsController.$inject = [
        'authService'
    ];

    function eventDescriptionActionsController(authService) {
        /* jshint validthis: true */
        var vm = this;

        vm.hasDatePassed = hasDatePassed;

        vm.currentUserId = authService.identity.userId;
        vm.hasEventAdminPermissions = authService.hasPermissions(['EVENT_ADMINISTRATION']);

        ///////

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }

    }
})();
