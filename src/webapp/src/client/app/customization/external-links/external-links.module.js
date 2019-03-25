(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.ExternalLinks', [
            'ui.router',
            'simoonaApp.Customization',
            'simoonaApp.Common'
        ])
        .config(config)
        .run(init);

    config.$inject = ['$stateProvider'];

    function config($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.Customization.ExternalLinks', {
                url: '/ExternalLinks',
                templateUrl: 'app/customization/external-links/external-links.html',
                controller: 'externalLinksController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'EXTERNALLINK_ADMINISTRATION'
                }
            });
    }

    init.$inject = ['customizationNavigationFactory'];

    function init(customizationNavigationFactory) {
        customizationNavigationFactory.defineCustomizationMenuItem({
            order: 1,
            permission: 'EXTERNALLINK_ADMINISTRATION',
            state: 'Root.WithOrg.Admin.Customization.ExternalLinks',
            iconName: 'glyphicon-new-window-alt',
            nameResource: 'customization.externalLinks',
            descriptionResource: 'customization.exteralLinksDescription',
            testId: 'external-links'
        });
    }
})();
