(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosUserSearchWidget', {
            bindings: {
                userFullName: '='
            },
            templateUrl: 'app/kudos/user-search-widget/user-search-widget.html',
            controller: kudosUserSearchWidgetController,
            controllerAs: 'vm'
        });

    kudosUserSearchWidgetController.$inject = [
        '$state',
        'kudosFactory',
        'authService'
    ];

    function kudosUserSearchWidgetController($state, kudosFactory, authService) {
        /*jshint validthis: true */
        var vm = this;

        vm.getUsersForAutocomplete = getUsersForAutocomplete;
        vm.selectUser = selectUser;

        vm.user = '';
        var userId = $state.params.userId || authService.identity.userId;

        init();

        /////////

        function init() {
        }

        function getUsersForAutocomplete(query) {
            return kudosFactory.getUsersForAutoComplete(query);
        }

        function selectUser(user) {
            $state.go('Root.WithOrg.Client.Kudos.KudosUserInformation', {
                userId: user.id
            }, {
                reload: true
            });
        }
    }
}());