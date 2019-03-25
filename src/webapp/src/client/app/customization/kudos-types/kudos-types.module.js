(function() {

    angular
        .module("simoonaApp.Customization.KudosTypes", [
            'ui.router',
            'simoonaApp.Customization',
            'simoonaApp.Common'
        ])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.Customization.KudosTypes', {
                abstract: true,
                url: '/KudosTypes',
                template: '<ui-view></ui-view>',
                data: {
                    authorizePermission: 'KUDOS_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.KudosTypes.List', {
                url: '',
                templateUrl: 'app/customization/kudos-types/list/list.html',
                controller: 'kudosTypesListController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'KUDOS_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.KudosTypes.Create', {
                url: '/Create',
                templateUrl: 'app/customization/kudos-types/create-edit/create-edit.html',
                controller: 'createEditKudosTypesController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'KUDOS_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.KudosTypes.Edit', {
                url: '/Edit/:id',
                templateUrl: 'app/customization/kudos-types/create-edit/create-edit.html',
                controller: 'createEditKudosTypesController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'KUDOS_ADMINISTRATION'
                }
            });
    }

    init.$inject = ['customizationNavigationFactory'];

    function init(customizationNavigationFactory) {
        customizationNavigationFactory.defineCustomizationMenuItem({
            order: 3,
            permission: 'KUDOS_ADMINISTRATION',
            state: 'Root.WithOrg.Admin.Customization.KudosTypes.List',
            iconName: 'glyphicon-certificate',
            nameResource: 'customization.kudosTypes',
            descriptionResource: 'customization.kudosTypesDescription',
            testId: 'kudos-types'
        });
    }

})();