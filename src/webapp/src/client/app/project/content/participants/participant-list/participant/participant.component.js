(function() {
    'use strict';

    angular
        .module('simoonaApp.Project')
        .component('aceProjectParticipant', {
            replace: true,
            transclude: true,
            bindings: {
                member: '='
            },
            templateUrl: 'app/project/content/participants/participant-list/participant/participant.html',
            controller: projectParticipantController,
            controllerAs: 'vm'
        });

    projectParticipantController.$inject = [
        'smallAvatarThumbSettings',
        'authService'
    ];

    function projectParticipantController(smallAvatarThumbSettings, authService) {
        /* jshint validthis: true */
        var vm = this;

        vm.currentUserId = authService.identity.userId;
        vm.participantThumbSettings = smallAvatarThumbSettings;
    }
})();
