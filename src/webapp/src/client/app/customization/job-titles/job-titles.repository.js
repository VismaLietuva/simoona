(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.JobTitles')
        .factory('jobTitlesRepository', jobTitlesRepository);

    jobTitlesRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function jobTitlesRepository($resource, endPoint) {
        var jobTitlesUrl = endPoint + '/JobType/';

        var service = {
            getJobTitles: getJobTitles,
            getJobTitle: getJobTitle,
            createJobTitle: createJobTitle,
            updateJobTitle: updateJobTitle,
            deleteJobTitle: deleteJobTitle
        };
        return service;

        ///////////

        function getJobTitles() {
            return $resource(jobTitlesUrl + 'Types').query().$promise;
        }

        function getJobTitle(id) {
            return $resource(jobTitlesUrl + 'Get').get({ id: id }).$promise;
        }

        function createJobTitle(jobTitle) {
            return $resource(jobTitlesUrl + 'Create').save(jobTitle).$promise;
        }

        function updateJobTitle(jobTitle) {
            return $resource(jobTitlesUrl + 'Update', '', {
                put: {
                    method: 'PUT'
                }
            }).put(jobTitle).$promise;
        }

        function deleteJobTitle(id) {
            return $resource(jobTitlesUrl + 'Delete').delete({ id: id }).$promise;
        }
    }
})();
