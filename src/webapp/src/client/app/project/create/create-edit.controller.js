(function () {
    'use strict';

    angular.module('simoonaApp.Project')
        .constant('projectSettings', {
            titleLength: 100,
            descriptionLength: 5000,
            thumbHeight: 165,
            thumbWidth: 291
        })
        .controller('projectCreateController', projectListController);

    projectListController.$inject = [
        '$rootScope',
        '$scope',
        'projectRepository',
        'pictureRepository',
        'authService',
        'dataHandler',
        'projectSettings',
        'errorHandler',
        '$stateParams',
        '$state',
        'lodash',
        'notifySrv'
    ];

    function projectListController($rootScope, $scope, projectRepository, pictureRepository, authService,
        dataHandler, projectSettings, errorHandler, $stateParams, $state, lodash, notifySrv) {
        /*jshint validthis: true */
        var vm = this;
        var isProjectsAdministrator = authService.hasPermissions(['PROJECT_ADMINISTRATION']);

        vm.showTemplate = false;

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Client.Projects.Create'),
            isEdit: $state.includes('Root.WithOrg.Client.Projects.Edit')
        };

        if (vm.states.isCreate) {
            $rootScope.pageTitle = 'projects.createProject';
        } else {
            $rootScope.pageTitle = 'projects.editProject';
        }

        vm.projectSettings = projectSettings;
        vm.isSaveButtonEnabled = true;
        
        vm.project = {};
        vm.project.attributes = [];
        vm.members = [];
        vm.attributes = [];
        vm.projectLogo = '';
        vm.projectCroppedLogo = '';
        vm.projectLogoSize = {
            w: projectSettings.thumbWidth,
            h: projectSettings.thumbHeight
        };

        vm.isOwnerError = false;
        vm.isLoading = true;

        vm.onTagAdded = onAttributeAdded;
        vm.onTagRemoved = onAttributeRemoved;
        vm.submitProjectForm = submitProjectForm;
        vm.searchAttributes = searchAttributes;
        vm.autocompleteUser = autocompleteUser;
        vm.createProject = createProject;
        vm.updateProject = updateProject;
        vm.deleteProject = deleteProject;

        init();
        /////////

        function init() {
            if ($stateParams.id) {
                loadProjectForUpdate($stateParams.id);
            } else {
                if (isProjectsAdministrator) {
                    vm.showTemplate = true;
                } else {
                    goToProjectsList();
                }
            }

            $scope.$watch('vm.project.owner', function(newVal) {
                if (newVal && !newVal.id) {
                    vm.isOwnerError = true;
                } else {
                    vm.isOwnerError = null;
                }
            }, true);
        }

        function loadProjectForUpdate(id) {
            projectRepository.getProjectById(id)
                .then(function(project) {
                    vm.project = project;
                    vm.project.owner = {id: project.owner.userId, fullName: project.owner.fullName };

                    lodash.each(project.attributes, function(attribute) {
                        vm.attributes.push({ title: attribute });
                    });

                    lodash.each(project.members, function(member) {
                        vm.members.push({ id: member.userId, fullName: member.fullName });
                    });

                    if(isProjectsAdministrator || project.owner.id === authService.identity.userId) {
                        vm.showTemplate = true;
                    } else {
                        goToProjectsList();
                    }
                }, function(error) {
                    errorHandler.handleErrorMessage(error);
                    goToProjectsList();
                });
        }

        function submitProjectForm(method, isImageChanged) {
            if (isImageChanged && vm.projectLogo && vm.projectCroppedLogo) {
                var projectLogoBlob = dataHandler.dataURItoBlob(vm.projectCroppedLogo, vm.projectLogo.type);

                projectLogoBlob.lastModifiedDate = new Date();
                projectLogoBlob.name = vm.projectLogo.name;
                var projectLogo = projectLogoBlob;

                pictureRepository.upload([projectLogoBlob]).then(function(result) {
                    method(result.data);
                });
            } else {
                method();
            }
        }

        function createProject(image) {
            if (vm.isSaveButtonEnabled) {
                vm.isSaveButtonEnabled = false;

                setMembers();
                setOwner();

                if (image) {
                    vm.project.logo = image;
                }

                projectRepository.createProject(vm.project)
                    .then(function(result) {
                        notifySrv.success('common.successfullySaved');
                        goToProjectsList();
                    },
                    function(error) {
                        vm.isSaveButtonEnabled = true;
                        errorHandler.handleErrorMessage(error);
                    });
            }
        }

        function updateProject(image) {
            if(vm.isSaveButtonEnabled) {
                vm.isSaveButtonEnabled = false;

                setMembers();
                setOwner();

                if (image) {
                    vm.project.logo = image;
                }

                projectRepository.updateProject(vm.project)
                    .then(function(result) {
                        notifySrv.success('common.successfullySaved');
                        goToProjectsList();
                    },
                    function(error) {
                        vm.isSaveButtonEnabled = true;
                        errorHandler.handleErrorMessage(error);
                    });
            }
        }

        function deleteProject() {
            projectRepository.deleteProject(vm.project.id)
                .then(function() {
                    goToProjectsList();
                }, function(error) {
                    errorHandler.handleErrorMessage(error);
                });
        }

        function onAttributeAdded(tag) {
            vm.project.attributes.push(tag.title);
        }
        
        function onAttributeRemoved(tag) {
            lodash.remove(vm.project.attributes, function(tech) {
                return tech === tag.title;
            });
        }

        function searchAttributes(query) {
            return projectRepository.getAttributesForAutoComplete({
                s: query
            });
        }

        function autocompleteUser(search) {
            return projectRepository.autocompleteUsers(search);
        }

        function setMembers() {
            vm.project.membersIds = lodash.map(vm.members, 'id');
        }

        function setOwner() {
            vm.project.owningUserId = vm.project.owner.id;
        }

        function goToProjectsList() {
            $state.go('Root.WithOrg.Client.Projects.List');
        }
    }
})();