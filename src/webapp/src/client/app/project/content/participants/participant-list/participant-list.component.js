(function() {
    'use strict';

    angular
        .module('simoonaApp.Project')
        .component('aceProjectParticipantList', {
            bindings: {
                members: '=',
                onLeaveProject: '&',
                isDeleteVisible: '='
            },
            templateUrl: 'app/project/content/participants/participant-list/participant-list.html',
            controller: projectParticipantListController,
            controllerAs: 'vm'
        });

    function projectParticipantListController() {
        /* jshint validthis: true */
        var vm = this;
    }
})();
