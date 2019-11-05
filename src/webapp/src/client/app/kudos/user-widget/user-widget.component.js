(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosUserWidget', {
            bindings: {
                user: '=',
                kudosifyUser: '=',
                isLoading: '='
            },
            templateUrl: 'app/kudos/user-widget/user-widget.html',
            controller: kudosUserWidgetController,
            controllerAs: 'vm'
        });

    kudosUserWidgetController.$inject = [
        '$state',
        'authService',
        'modalTypes'
    ];

    function kudosUserWidgetController($state, authService, modalTypes) {
        /*jshint validthis: true */
        var vm = this;

        vm.userId = $state.params.userId || authService.identity.userId;
        vm.modalTypes = modalTypes;
        init();

        ////////

        function init() {
        }
    }
}());
