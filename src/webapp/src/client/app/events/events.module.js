(function () {
    'use strict';

    window.modules = window.modules || [];
    window.modules.push('simoonaApp.Events');

    angular
        .module('simoonaApp.Events', [
            'ui.router',
            'ngLodash',
            'infinite-scroll',
            'angular-spinkit',
            'ngImgCrop',
            'pascalprecht.translate',
            'simoonaApp.Common',
            'simoonaApp.Layout.LeftMenu'
        ])
        .constant('optionRules', {
            default: 0,
            ignoreSingleJoin: 1
        })
        .constant('defaultEventTabsNames', {
            all: 'all',
            main: 'main',
            participant: 'participant',
            host: 'host'
        })
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider', '$windowProvider'];

    function route($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }
        $stateProvider
            .state('Root.WithOrg.Client.Events', {
                abstract: true,
                url: '/Events',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Client.Events.List', {
                url: '/List',
                templateUrl: 'app/events/list/list.html',
                controller: 'eventsListController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Events.List.Type', {
                url: '/:type/office/:office',
                templateUrl: 'app/events/list/by-type/by-type.html',
                controller: 'eventsByTypeController',
                controllerAs: 'vm',
                params: {
                    type: 'main',
                    office: 'all',
                    startDate: { squash: true, value: null },
                    endDate: { squash: true, value: null }
                }
            })
            .state('Root.WithOrg.Client.Events.EventContent', {
                url: '/EventContent/:id',
                templateUrl: 'app/events/content/content.html',
                controller: 'EventContentController',
                controllerAs: 'vm',
                params: {
                    postNotification: null
                }
            })
            .state('Root.WithOrg.Client.Events.AddEvents', {
                url: '/AddEvent',
                templateUrl: 'app/events/create-edit/create-edit.html',
                controller: 'addNewEventController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Events.EditEvent', {
                url: '/Edit/:id',
                templateUrl: 'app/events/create-edit/create-edit.html',
                controller: 'addNewEventController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Events.Details', {
                url: '/Details',
                templateUrl: 'app/events/details-list/details-list.html',
                controller: 'eventDetailsListController',
                controllerAs: 'vm'
            })
            .state('Root.WithOrg.Client.Events.Details.Event', {
                parent: 'Root.WithOrg.Client.Events',
                url: '/Event/:id',
                templateUrl: 'app/events/details-list/event-details/event-details.html',
                controller: 'eventDetailsController',
                controllerAs: 'vm',
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
            permission: 'EVENT_BASIC',
            url: 'Root.WithOrg.Client.Events.List.Type',
            active: 'Root.WithOrg.Client.Events',
            resource: 'navbar.events',
            order: 1,
            group: leftMenuGroups.activities
        });
    }
})();
