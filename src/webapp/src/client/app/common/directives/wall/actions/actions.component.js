(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceMessageActions', {
            bindings: {
                messageUserId: '=',
                isAdmin: '=',
                canModerateComment: '=',
                canModeratePost: '=',
                enableEditor: '&',
                deleteMessage: '&',
                currentWallId: '=',
                isWallModule: '=',
                canDelete: '='
            },
            templateUrl: 'app/common/directives/wall/actions/actions.html',
            controller: messageActionsController,
            controllerAs: 'vm'
        });

    messageActionsController.$inject = [
        'authService'
    ];

    function messageActionsController(authService) {
        /*jshint validthis: true */
        var vm = this;

        vm.isMessageOwner = vm.messageUserId === authService.identity.userId;

    }
}());
