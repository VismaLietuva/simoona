(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventParticipantOptions', {
            bindings: {
                options: '=',
                participants: '='
            },
            templateUrl: 'app/events/content/participants/participant-options/participant-options.html',
            controller: eventParticipantOptionsController,
            controllerAs: 'vm'
        });

    eventParticipantOptionsController.$inject = [
        'authService',
        'lodash'
    ];

    function eventParticipantOptionsController(authService, lodash) {
        /* jshint validthis: true */
        var vm = this;

        vm.accordionOptionArray = [];

        vm.hasCurrentUserSelectedOption = hasCurrentUserSelectedOption;

        ////////

        function hasCurrentUserSelectedOption(participants) {
            var currentUserId = authService.identity.userId;

            return !!lodash.find(participants, function(obj) {
                return obj.userId === currentUserId;
            });
        }
    }
})();
