(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.EventTypes', [
            'ui.router',
            'simoonaApp.Customization',
            'simoonaApp.Common'
        ])
        .config(config)
        .run(init);

    config.$inject = ['$stateProvider', '$windowProvider'];

    function config($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }
        $stateProvider
            .state('Root.WithOrg.Admin.Customization.EventTypes', {
                abstract: true,
                url: '/EventTypes',
                template: '<ui-view></ui-view>',
                data: {
                    authorizePermission: 'EVENT_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.EventTypes.List', {
                url: '',
                templateUrl: 'app/customization/event-types/event-types.html',
                controller: 'eventTypesController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'EVENT_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.EventTypes.Create', {
                url: '/Create',
                templateUrl: 'app/customization/event-types/create-edit/create-edit.html',
                controller: 'eventTypesCreateController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'EVENT_ADMINISTRATION'
                }
            }).state('Root.WithOrg.Admin.Customization.EventTypes.Edit', {
                url: '/Edit/:id',
                templateUrl: 'app/customization/event-types/create-edit/create-edit.html',
                controller: 'eventTypesCreateController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'EVENT_ADMINISTRATION'
                }
            });
    }

    init.$inject = ['customizationNavigationFactory', '$window'];

    function init(customizationNavigationFactory, $window) {
        if (!$window.isPremium) {
            return;
        }
        customizationNavigationFactory.defineCustomizationMenuItem({
            order: 2,
            permission: 'EVENT_ADMINISTRATION',
            state: 'Root.WithOrg.Admin.Customization.EventTypes.List',
            iconName: 'glyphicon-tags',
            nameResource: 'customization.eventTypes',
            descriptionResource: 'customization.eventTypesDescription',
            testId: 'event-types'
        });
    }
})();
