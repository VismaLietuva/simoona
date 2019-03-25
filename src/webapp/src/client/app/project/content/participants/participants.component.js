(function() {
    'use strict';

    angular
        .module('simoonaApp.Project')
        .component('aceProjectParticipants', {
            bindings: {
                project: '=',
                isAdmin: '=',
                isLoading: '='
            },
            templateUrl: 'app/project/content/participants/participants.html',
            controller: projectParticipantsController,
            controllerAs: 'vm'
        });

    projectParticipantsController.$inject = [
        'projectRepository',
        'errorHandler',
        'lodash',
        'Analytics',
        '$stateParams'
    ];

    function projectParticipantsController(projectRepository, 
        errorHandler, lodash, Analytics, $stateParams) {
        /* jshint validthis: true */
        var vm = this;

        vm.expelUserFromProject = expelUserFromProject;
        vm.projectId = $stateParams.id;

        /////////

        function expelUserFromProject(member) {
            Analytics.trackEvent('Projects', 'expelUserFromProject: ' + member.id, 'Project: ' + vm.projectId);
            if (!vm.project.isLoading) {
                vm.project.isLoading = true;

                projectRepository.expelUserFromProject(vm.projectId, member.id).then(function() {
                    removeMember(vm.project.members, member.id);
                    vm.project.membersCount = vm.project.members.length;
                    vm.project.isLoading = false;
                }, function(response) {
                    vm.project.isLoading = false;
                    errorHandler.handleErrorMessage(response, 'expelMember');
                });
            }
        }

        function removeMember(members, userId)
        {
            lodash.remove(members, {
                id: userId
            });
        }
    }
})();
