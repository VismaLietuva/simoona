(function () {
    'use strict';

    angular.module('simoonaApp.Recommendation', [
        'ui.router',
        ])
        .config(route);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
        .state('Root.WithOrg.Client.FriendRecommendation', {
            url: '/FriendRecommendation',
            templateUrl: 'app/recommendation/friend-recommendation/friend-recommendation.html',
            controller: 'recommendationController',
            controllerAs: 'vm'
        });
    }
})();