(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.ServiceRequestsTypes', [
            'ui.router',
            'simoonaApp.Customization',
            'simoonaApp.Common'
        ])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider', '$windowProvider'];

    function route($stateProvider, $windowProvider) {
        if (!$windowProvider.$get().isPremium) {
            return;
        }
        $stateProvider
            .state('Root.WithOrg.Admin.Customization.ServiceRequestsTypes', {
                abstract: true,
                url: '/ServiceRequestsTypes',
                template: '<ui-view></ui-view>',
                data: {
                    authorizePermission: 'SERVICEREQUESTS_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.ServiceRequestsTypes.List', {
                url: '',
                templateUrl: 'app/customization/service-request-types/list/list.html',
                controller: 'serviceRequestsTypesListController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'SERVICEREQUESTS_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.ServiceRequestsTypes.Create', {
                url: '/Create',
                templateUrl: 'app/customization/service-request-types/create-edit/create-edit.html',
                controller: 'serviceRequestsTypesCreateEditController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'SERVICEREQUESTS_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.ServiceRequestsTypes.Edit', {
                url: '/Edit/:id',
                templateUrl: 'app/customization/service-request-types/create-edit/create-edit.html',
                controller: 'serviceRequestsTypesCreateEditController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'SERVICEREQUESTS_ADMINISTRATION'
                }
            });
    }

    init.$inject = ['customizationNavigationFactory', '$window'];

    function init(customizationNavigationFactory, $window) {
        if (!$window.isPremium) {
            return;
        }
        customizationNavigationFactory.defineCustomizationMenuItem({
            order: 4,
            permission: 'SERVICEREQUESTS_ADMINISTRATION',
            state: 'Root.WithOrg.Admin.Customization.ServiceRequestsTypes.List',
            iconName: 'glyphicon-phone-alt',
            nameResource: 'customization.serviceRequestsTypes',
            descriptionResource: 'customization.serviceRequestsTypesDescription',
            testId: 'service-request-types'
        });
    }

})();