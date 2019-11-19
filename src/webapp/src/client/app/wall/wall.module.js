(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall', [
            'ui.router',
            'simoonaApp.Common',
            'ngImgCrop'
        ])
        .config(route);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Client.Wall', {
                abstract: true,
                url: '/Wall',
                templateUrl: 'app/wall/wall.html',
                controller: 'wallController',
                controllerAs: 'vm'
              
            })
            .state('Root.WithOrg.Client.Wall.Item', {
                abstract: true,
                url: '',
                templateUrl: 'app/wall/item/item.html',
                controller: 'wallItemController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Wall.Item.Feed', {
                url: '/Feed?wall/?search/?post',
                reloadOnSearch: false,
                templateUrl: 'app/wall/item/feed/feed.html',
                controller: 'wallFeedController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Wall.Item.Members', {
                url: '/Members?wall',
                templateUrl: 'app/wall/item/members/members.html',
                controller: 'wallMembersController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Wall.All', {
                url: '/All',
                templateUrl: 'app/wall/item/feed/feed.html',
            })
            .state('Root.WithOrg.Client.Wall.List', {
                url: '/List',
                templateUrl: 'app/wall/discover-walls/list/list.html',
                controller: 'discoverWallsListController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Wall.Create', {
                url: '/Create',
                templateUrl: 'app/wall/discover-walls/create-edit/create-edit.html',
                controller: 'discoverWallsCreateController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Wall.Edit', {
                url: '/Edit/:id',
                templateUrl: 'app/wall/discover-walls/create-edit/create-edit.html',
                controller: 'discoverWallsCreateController',
                controllerAs: 'vm'
            });
    }
}());
