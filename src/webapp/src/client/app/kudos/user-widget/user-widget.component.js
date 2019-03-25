(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosUserWidget', {
            templateUrl: 'app/kudos/user-widget/user-widget.html',
            controller: kudosUserWidgetController,
            controllerAs: 'vm'
        });

    kudosUserWidgetController.$inject = [
        '$state',
        'authService',
        'kudosFactory'
    ];

    function kudosUserWidgetController($state, authService, kudosFactory) {
        /*jshint validthis: true */
        var vm = this;

        vm.userId = $state.params.userId || authService.identity.userId;
        vm.isLoading = true;

        init();

        ////////

        function init() {
            kudosFactory.getUserInformation(vm.userId).then(function (response) {
                vm.user = response;
                vm.kudosifyUser = {
                    formattedName: response.firstName + ' ' + response.lastName,
                    id: vm.userId
                };
                vm.isLoading = false;
            });
        }
    }
}());
