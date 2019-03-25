(function () {
    'use strict';

    angular
        .module('simoonaApp.Project')
        .factory('projectRepository', projectRepository);

    projectRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function projectRepository($resource, endPoint) {
        
        var skillUrl = endPoint + '/Skill/';
        var projectUrl = endPoint + '/Project/';
        var applicationUrl = endPoint + '/ApplicationUser/';

        var service = {
            getProjectList : getProjectList,
            getForAutocomplete : getForAutocomplete,
            getProjectDetails : getProjectDetails,
            expelUserFromProject : expelUserFromProject,         
            createProject: createProject,
            updateProject: updateProject,
            deleteProject: deleteProject,
            getAttributesForAutoComplete: getAttributesForAutoComplete,
            autocompleteUsers: autocompleteUsers,
            getProjectById: getProjectById
        };

        return service;

        /////////

        function getProjectList() {
            return $resource(projectUrl + 'List').query().$promise;
        }

        function getForAutocomplete(params) {
            return $resource(projectUrl + 'AutoComplete').query(params).$promise;
        }

        function getProjectDetails(projectId) {
            return $resource(projectUrl + 'Details').get({ projectId: projectId }).$promise;
        }

        function expelUserFromProject(projectId, userId) {
            return $resource(projectUrl + 'ExpelMember').delete({ projectId: projectId, userId : userId }).$promise;
        }

        function getAttributesForAutoComplete(params) {
            return $resource(skillUrl + 'GetForAutoComplete').query(params).$promise;
        }

        function autocompleteUsers(params) {
            return $resource(applicationUrl + 'GetForAutoComplete').query({
                s: params
            }).$promise;
        }

        function createProject(project) {
            return $resource(projectUrl + 'Create').save(project).$promise;
        }

        function updateProject(project) {
            return $resource(projectUrl + 'Edit', '', {put: {method: 'PUT'}}).put(project).$promise;
        }

        function deleteProject(id) {
            return $resource(projectUrl + 'Delete').delete({ id: id }).$promise;
        }

        function getProjectById(id) {
            return $resource(projectUrl + 'Edit').get({ id: id }).$promise;
        }
    }
})();