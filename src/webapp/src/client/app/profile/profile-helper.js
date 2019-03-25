(function () {

    'use strict';

    angular
        .module('simoonaApp.Profile')
        .service('profileHelper', profileHelper);

    function profileHelper() {

        var service = {
            getTitleAndQualification: getTitleAndQualification,
            getFullName: getFullName,
            getProjectsCsv: getProjectsCsv

        }

        function getTitleAndQualification(profile) {
            var returnValue = '';

            if (profile.jobTitle && !profile.qualificationLevelName) {
                returnValue = profile.jobTitle;
            }

            if (!profile.jobTitle && profile.qualificationLevelName) {
                returnValue = profile.qualificationLevelName;
            }

            if (profile.jobTitle && profile.qualificationLevelName) {
                returnValue = profile.jobTitle + ' (' + profile.qualificationLevelName + ')';
            }

            return returnValue;
        }

        function getFullName(profile) {
            //show username if both first name and last name are missing
            var returnValue = '';

            if (profile.firstName && !profile.lastName) {
                returnValue = profile.firstName;
            }

            if (!profile.firstName && profile.lastName) {
                returnValue = profile.lastName;
            }

            if (profile.firstName && profile.lastName) {
                returnValue = profile.firstName + ' ' + profile.lastName;
            }

            if (!profile.firstName && !profile.lastName) {
                returnValue = profile.userName;
            }
            return returnValue;
        }

        function getProjectsCsv(projects) {
            var projectNames = [];

            angular.forEach(projects, function (project, key) {
                projectNames.push(project.name);
            });

            return projectNames.join(', ');
        }

        return service;
    }

})();