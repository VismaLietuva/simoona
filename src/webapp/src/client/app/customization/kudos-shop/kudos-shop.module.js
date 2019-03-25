(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.KudosShop', [
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
            .state('Root.WithOrg.Admin.Customization.KudosShop', {
                abstract: true,
                url: '/KudosShop',
                template: '<ui-view></ui-view>',
                data: {
                    authorizePermission: 'KUDOSSHOP_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.KudosShop.List', {
                url: '',
                templateUrl: 'app/customization/kudos-shop/kudos-shop.html',
                controller: 'kudosShopController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'KUDOSSHOP_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.KudosShop.Create', {
                url: '/Create',
                templateUrl: 'app/customization/kudos-shop/create-edit/create-edit.html',
                controller: 'kudosShopCreateController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'KUDOSSHOP_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.KudosShop.Edit', {
                url: '/Edit/:id',
                templateUrl: 'app/customization/kudos-shop/create-edit/create-edit.html',
                controller: 'kudosShopCreateController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'KUDOSSHOP_ADMINISTRATION'
                }
            });
    }

    init.$inject = ['customizationNavigationFactory', '$window'];

    function init(customizationNavigationFactory, $window) {
        if (!$window.isPremium) {
            return;
        }
        customizationNavigationFactory.defineCustomizationMenuItem({
            order: 7,
            permission: 'KUDOSSHOP_ADMINISTRATION',
            state: 'Root.WithOrg.Admin.Customization.KudosShop.List',
            iconName: 'glyphicon-shopping-cart',
            nameResource: 'customization.kudosShop',
            descriptionResource: 'customization.kudosShopDescription'
        });
    }
})();