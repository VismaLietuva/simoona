(function () {
    'use strict';

    angular.module('simoonaApp.Support', [
        'ui.router',
        ])
        .config(route);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
        .state('Root.WithOrg.Client.SubmitTicket', {
            url: '/SubmitTicket',
            templateUrl: 'app/support/submit-ticket/submit-ticket.html',
            controller: 'supportController',
            controllerAs: 'vm'
        });
    }
})();
