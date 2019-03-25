(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.OrganizationSettings', [
            'ui.router',
            'simoonaApp.Customization',
            'simoonaApp.Common'
        ])
        .config(config)
        .run(init);

    config.$inject = ['$stateProvider'];

    function config($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.Customization.OrganizationSettings', {
                url: '/OrganizationSettings',
                templateUrl: 'app/customization/organization-settings/organization-settings.html',
                controller: 'organizationSettingsController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'ORGANIZATION_ADMINISTRATION'
                }
            });
    }

    init.$inject = ['customizationNavigationFactory'];

    function init(customizationNavigationFactory) {
        customizationNavigationFactory.defineCustomizationMenuItem({
            order: 5,
            permission: 'ORGANIZATION_ADMINISTRATION',
            state: 'Root.WithOrg.Admin.Customization.OrganizationSettings',
            iconName: 'glyphicon-user-structure',
            nameResource: 'customization.organizationSettings',
            descriptionResource: 'customization.organizationSettingsDescription',
            testId: 'organization-settings'
        });
    }
})();
