(function() {
    'use strict';

    angular
        .module('simoonaApp.Project')
        .component('aceProjectDescriptionActions', {
            bindings: {
                project: '='
            },
            templateUrl: 'app/project/content/description/description-actions/description-actions.html',
            controller: projectDescriptionActionsController,
            controllerAs: 'vm'
        });

    projectDescriptionActionsController.$inject = [
        '$state',
        'projectRepository',
        'authService',
        'notifySrv',
        'errorHandler'
    ];

    function projectDescriptionActionsController($state, projectRepository, authService,
        notifySrv, errorHandler) {
        /* jshint validthis: true */
        var vm = this;

        vm.deleteproject = deleteProject;

        vm.currentUserId = authService.identity.userId;
        vm.currentProjectId = $state.params.id;
        vm.hasProjectAdminPermissions = authService.hasPermissions(['PROJECT_ADMINISTRATION']);

        ///////

        function deleteProject(id) {
            projectRepository.deleteProject(id).then(function(result) {
                notifySrv.success('projects.successDelete');

                $state.go('Root.WithOrg.Client.projects.List.Type', {type: 'all'});
            }, errorHandler.handleErrorMessage);
        }
    }
})();
