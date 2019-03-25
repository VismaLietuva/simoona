(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceMessageLike', {
            replace: true,
            templateUrl: 'app/common/directives/wall/message-like/message-like.html',
            bindings: {
                messageObject: '=',
                type: '@'
            },
            controller: wallLikeController,
            controllerAs: 'vm'
        });

    wallLikeController.$inject = [];

    function wallLikeController() {
        /*jshint validthis: true */
        var vm = this;
    }

}());