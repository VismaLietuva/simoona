(function () {
    'use strict';

    angular
        .module('simoonaApp.Widget.KudosBasket', ['ui.router'])
        .config(route);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.KudosBasket', {
                url: '/KudosBasket',
                templateUrl: 'app/widget/kudos-basket/kudos-basket.html',
                controller: 'KudosBasketController',
                controllerAs: 'vm',
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'KUDOSBASKET_ADMINISTRATION'
                }
            });
    }
}());
