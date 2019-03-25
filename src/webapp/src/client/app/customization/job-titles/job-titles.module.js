(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.JobTitles', [
            'ui.router',
            'simoonaApp.Customization',
            'simoonaApp.Common'
        ])
        .config(config)
        .run(init);

    config.$inject = ['$stateProvider'];

    function config($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Admin.Customization.JobTitles', {
                abstract: true,
                url: '/JobTitles',
                template: '<ui-view></ui-view>',
                data: {
                    authorizePermission: 'JOB_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Customization.JobTitles.List', {
                url: '',
                templateUrl: 'app/customization/job-titles/job-titles.html',
                controller: 'jobTitlesController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'JOB_ADMINISTRATION'
                }
           })
            .state('Root.WithOrg.Admin.Customization.JobTitles.Create', {
                url: '/Create',
                templateUrl: 'app/customization/job-titles/create-edit/create-edit.html',
                controller: 'jobTitlesCreateController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'JOB_ADMINISTRATION'
                }
            })
			.state('Root.WithOrg.Admin.Customization.JobTitles.Edit', {
                url: '/Edit/:id',
                templateUrl: 'app/customization/job-titles/create-edit/create-edit.html',
                controller: 'jobTitlesCreateController',
                controllerAs: 'vm',
                data: {
                    authorizePermission: 'JOB_ADMINISTRATION'
                }
            });
    }

init.$inject = ['customizationNavigationFactory'];
    function init(customizationNavigationFactory) {
            customizationNavigationFactory.defineCustomizationMenuItem({
            order: 6,
            permission: 'JOB_ADMINISTRATION',
            state: 'Root.WithOrg.Admin.Customization.JobTitles.List',
            iconName: 'glyphicon-nameplate-alt',
            nameResource: 'customization.jobTitles',
            descriptionResource: 'customization.jobTitlesDescription',
            testId: 'job-titles'
        });
    }
})();
