(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('eventParticipantThumbSettings', {
            width: 35,
            height: 35,
            mode: 'crop'
        })
        .component('aceEventParticipant', {
            replace: true,
            transclude: true,
            bindings: {
                participant: '='
            },
            templateUrl: 'app/events/content/participants/participant-list/participant/participant.html',
            controller: eventParticipantController,
            controllerAs: 'vm'
        });

    eventParticipantController.$inject = [
        'eventParticipantThumbSettings',
        'authService'
    ];

    function eventParticipantController(eventParticipantThumbSettings, authService) {
        /* jshint validthis: true */
        var vm = this;

        vm.currentUserId = authService.identity.userId;
        vm.participantThumbSettings = eventParticipantThumbSettings;
    }
})();
