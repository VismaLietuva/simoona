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
        'authService',
        'eventRepository',
        'notifySrv',
        'localeSrv',
        'errorHandler'
    ];

    function eventDescriptionActionsController(authService, eventRepository, notifySrv , localeSrv, errorHandler) {
        /* jshint validthis: true */
        var vm = this;

        vm.hasDatePassed = hasDatePassed;
        vm.togglePin = togglePin;
        vm.localeSrv = localeSrv;
        vm.notifySrv = notifySrv;
        vm.isPinned = vm.event.isPinned;
        vm.currentUserId = authService.identity.userId;
        vm.hasEventAdminPermissions = authService.hasPermissions(['EVENT_ADMINISTRATION']);

        ///////

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }
        
        function togglePin(event) {
            eventRepository.pinEvent(event.id)
            .then(function(){
                vm.notifySrv.success(vm.localeSrv.formatTranslation('lotteries.hasBeenBought'));
                vm.isPinned = !vm.isPinned
            }, function (error) {
                errorHandler.handleErrorMessage(error);
            });
        }

    }
})();
