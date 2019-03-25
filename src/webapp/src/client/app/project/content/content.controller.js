(function () {
    'use strict';

    angular
        .module('simoonaApp.Project')
        .controller('projectContentController', projectContentController);

    projectContentController.$inject = [
        '$rootScope',
        '$stateParams',
        '$state',
        '$location',
        '$anchorScroll',
        '$timeout',
        'projectRepository',
        'authService',
        'errorHandler'
    ];

    function projectContentController($rootScope, $stateParams, $state, $location, $anchorScroll, 
        $timeout, projectRepository, authService, errorHandler) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'projects.projectListTitle';

        vm.project = {};
        var projectId = $stateParams.id;

        vm.isProjectAdmin = false;
        vm.isProjectLoading = false;
        vm.currentUser = authService.identity.userId;
        vm.hasAdministrationPermission = authService.hasPermissions(['PROJECT_ADMINISTRATION']);

        vm.getProject = getProject;

        init();

        /////////

        function init() {
            vm.getProject();
        }

        function getProject() {
            vm.isProjectLoading = true;
            projectRepository.getProjectDetails(projectId).then(function(response) {
                vm.project = response;
                vm.project.membersCount = vm.project.members.length;
                vm.isProjectAdmin = vm.hasAdministrationPermission || vm.project.owner.id === vm.currentUser;
                vm.isProjectLoading = false;
            },
            function(error) {
                errorHandler.handleErrorMessage(error);

                $state.go('Root.WithOrg.Client.Projects.List');
            });
        }
    }
})();