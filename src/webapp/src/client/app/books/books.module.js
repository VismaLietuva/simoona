(function () {
    'use strict';

    window.modules = window.modules || [];
    window.modules.push('simoonaApp.Books');

    angular
        .module('simoonaApp.Books', [
            'ngLodash',
            'ui.router',
            'pascalprecht.translate',
            'simoonaApp.Common',
            'simoonaApp.Layout.LeftMenu'
        ])
        .config(route)
        .run(init);

    route.$inject = [
        '$stateProvider',
        '$windowProvider'
    ];

    function route($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }
        $stateProvider
            .state('Root.WithOrg.Client.Books', {
                abstract: true,
                url: '/Books',
                template: '<ui-view></ui-view>',
                data: {
                    authorizePermission: 'BOOK_BASIC'
                }
            })
            .state('Root.WithOrg.Client.Books.List', {
                url: '/List',
                controller: 'booksController',
                controllerAs: 'vm',
                templateUrl: 'app/books/books.html'
            })
            .state('Root.WithOrg.Client.Books.Add', {
                url: '/Add',
                controller: 'booksCreateEditController',
                controllerAs: 'vm',
                templateUrl: 'app/books/create-edit/create-edit.html'
            })
            .state('Root.WithOrg.Client.Books.Edit', {
                url: '/Edit/:id/:officeId',
                controller: 'booksCreateEditController',
                controllerAs: 'vm',
                templateUrl: 'app/books/create-edit/create-edit.html'
            });
    }

    init.$inject = [
        'menuNavigationFactory',
        'leftMenuGroups',
        '$window'
    ];

    function init(menuNavigationFactory, leftMenuGroups, $window) {
        if (!$window.isPremium) {
            return;
        }
        menuNavigationFactory.defineLeftMenuItem({
            permission: 'BOOK_BASIC',
            url: 'Root.WithOrg.Client.Books.List',
            active: 'Root.WithOrg.Client.Books',
            resource: 'navbar.books',
            order: 4,
            group: leftMenuGroups.activities
        });
    }
})();
