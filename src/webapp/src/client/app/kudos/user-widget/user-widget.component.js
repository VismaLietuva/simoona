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
        'authService'
    ];

    function kudosUserWidgetController($state, authService) {
        /*jshint validthis: true */
        var vm = this;

        vm.userId = $state.params.userId || authService.identity.userId;

        init();

        ////////

        function init() {
        }
    }
}());
