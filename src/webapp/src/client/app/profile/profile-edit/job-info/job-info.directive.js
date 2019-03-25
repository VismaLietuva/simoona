(function() {
    'use strict';

    angular
        .module('simoonaApp.Profile')
        .directive('jobInfo', jobInfo);

    function jobInfo() {

        return {
            templateUrl: 'app/profile/profile-edit/job-info/job-info.html',
            restrict: 'AE',
            scope: {
                model: '='
            },
            controller: controller
        };
    }

    controller.$inject = [
        '$scope',
        '$filter',
        '$timeout',
        'authService',
        'notifySrv',
        'userRepository',
        'projectRepository',
        'qualificationLevelRepository',
        'roleRepository',
        'profileRepository',
        '$stateParams',
        '$uibModal',
        'timepickerConvertions',
        'localeSrv',
        'uibDateParser'
    ];

    function controller($scope, $filter, $timeout, authService, notifySrv, userRepository, projectRepository,
        qualificationLevelRepository, roleRepository, profileRepository, $stateParams, $uibModal,
        timepickerConvertions, localeSrv, uibDateParser) {

        $scope.errors = [];
        $scope.infos = [];

        $scope.info = {};

        $scope.datePickers = {
            employmentDate: false
        };

        $scope.birthdayDatepickerOpened = true;

        $scope.toggleDatePicker = toggleDatePicker;
        $scope.openDatePicker = openDatePicker;

        $scope.addSkillTag = addSkillTag;

        $scope.allProjects = allProjects;
        $scope.allSkills = allSkills;

        $scope.allRoles = allRoles;
        $scope.privateFields = []; // Temporary. Should be set to response's private fields.
        $scope.isNewUser = authService.identity.isAuthenticated && authService.isInRole('NewUser');
        $scope.organizationName = authService.identity.organizationName;
        $scope.isAdmin = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);
        $scope.isAccountAdministrator = authService.hasPermissions(['ACCOUNT_ADMINISTRATION']);

        $scope.openExamModal = openExamModal;
        $scope.openExamDeleteModal = openExamDeleteModal;

        $scope.info = $scope.model;
        $scope.info.employmentDate = new Date($scope.info.employmentDate);
        
        if (!$scope.info.workingHours) {
            $scope.info.workingHours = {};
        }

        $scope.startTime = timepickerConvertions.covertToDateObject($scope.info.workingHours.startTime);
        $scope.endTime = timepickerConvertions.covertToDateObject($scope.info.workingHours.endTime);
        $scope.lunchStart = timepickerConvertions.covertToDateObject($scope.info.workingHours.lunchStart);
        $scope.lunchEnd = timepickerConvertions.covertToDateObject($scope.info.workingHours.lunchEnd);

        $scope.workingHoursResource = [{
            resource: 'common.yes',
            value: true
        }, {
            resource: 'common.no',
            value: false
        }];

        $scope.submitJobInfo = submitJobInfo;

        $scope.managers = function(search) {
            return userRepository.getManagersForAutocomplete(search, $scope.info.id);
        }

        $scope.getPartTimeHours = profileRepository.getPartTimeHours().then(function(response) {
            $scope.partTimeHours = response;
        })

        if (authService.hasPermissions(['QUALIFICATIONLEVEL_BASIC']))
            qualificationLevelRepository.getAll().then(function(response) {
                $scope.qualificationLevels = response;
            });

        function submitJobInfo() {
            $timeout(function() {
                convertTimeToString();

                var jobInfo = angular.copy($scope.info);

                jobInfo.projectIds = mapIds(jobInfo.projects);
                jobInfo.skillIds = mapIds(jobInfo.skills);
                jobInfo.roleIds = mapIds(jobInfo.roles);

                jobInfo.managerId = jobInfo.manager ? jobInfo.manager.id : null;

                jobInfo.qualificationLevelId = jobInfo.qualificationLevel ? jobInfo.qualificationLevel.id : null;

                jobInfo.jobPositionId = jobInfo.currentJobPosition ? jobInfo.currentJobPosition.id : null;

                profileRepository.putJobInfo(jobInfo).then(onSuccess, function(error) {
                    notifySrv.error('kudos.kudosifyModalError');
                });
            }, 500);

        }

        function convertTimeToString() {
            $scope.info.workingHours.startTime = timepickerConvertions.convertToString($scope.startTime);
            $scope.info.workingHours.endTime = timepickerConvertions.convertToString($scope.endTime);
            $scope.info.workingHours.lunchStart = timepickerConvertions.convertToString($scope.lunchStart);
            $scope.info.workingHours.lunchEnd = timepickerConvertions.convertToString($scope.lunchEnd);
        }

        function openExamModal(certificate) {
            if (!certificate)
                $scope.post = true;
            else
                $scope.post = false;

            var cachedCertificate = angular.copy(certificate);
            var modalInstance = $uibModal.open({
                templateUrl: 'app/profile/profile-edit/job-info/exams-modal/exams-modal.html',
                controller: 'examsModalCtrl',
                resolve: {
                    certificate: [function() {
                        return cachedCertificate;
                    }]
                }
            });

            modalInstance.result.then(onSave, onCancel);

            function onSave(certificate) {
                certificate.applicationUserId = $scope.info.id;
                if (certificate.exams) {
                    saveCertificateAndExams(certificate);
                } else {
                    saveCertificate(certificate);
                }
            }

            function onCancel() {}
        }

        function saveCertificateAndExams(certificate) {
            addExams(certificate.exams).then(function(response) {
                certificate.exams = [];
                angular.forEach(response, function(value) {
                    if (value.title)
                        certificate.exams.push(value);
                }, onError);
                saveCertificate(certificate);
            })
        }

        function saveCertificate(certificate) {
            addCertificate(certificate).then(function(response) {
                if ($scope.info.certificates) {
                    var index = $filter('filter')($scope.info.certificates, {
                        id: response.id
                    }, true);
                    if (index.length > 0)
                        $scope.info.certificates.remove(index[0]);
                }

                $scope.info.certificates.push(response);
                addExamsToUsers();
            }, onError);
        }

        function openExamDeleteModal(certificate) {
            var modalInstance = $uibModal.open({
                templateUrl: 'app/profile/profile-edit/job-info/exams-modal/delete-exam.html',
                controller: 'deleteExamCtrl',
                resolve: {
                    certificate: [function() {
                        return certificate;
                    }]
                }
            });

            modalInstance.result.then(onYes, onCancel);

            function onYes() {
                $scope.info.certificates.remove(certificate);
                profileRepository.deleteCertificate(certificate.id).then(function() {
                    notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeletedOneParam', { one: certificate.name }));
                }, onError);
                addExamsToUsers();
            }

            function onCancel() {}
        }

        function allProjects(search) {
            return projectRepository.getForAutocomplete({
                name: search
            });
        }

        function allSkills(search) {
            return profileRepository.getSkillForAutoComplete({
                s: search
            });
        }

        function allRoles(search) {
            return roleRepository.getRolesForAutoComplete(search);
        }

        function toggleDatePicker(key, $event) {
            $event.preventDefault();
            $event.stopPropagation();

            $scope.datePickers[key] = true;
        }

        function addSkillTag(tag) {
            if (!tag.id) {
                profileRepository.postSkill(tag).then(function(response) {
                    tag.id = response.id;
                    tag.title = response.title;
                }, onErrorResponse);
            }
        }

        function addCertificate(certificate) {
            if ($scope.post)
                return profileRepository.postCertificate(certificate);
            else
                return profileRepository.putCertificate(certificate);
        }

        function addExamsToUsers() {
            $scope.info.exams = [];
            angular.forEach($scope.info.certificates, function(value) {
                $scope.info.exams = $scope.info.exams.concat(value.exams);
            });
            var examIds = mapIds($scope.info.exams);
            profileRepository.putExams({
                userId: $scope.info.id,
                examIds: examIds
            }).then(onSuccess, onError);
        }

        function addExams(exams) {
            return profileRepository.postExam(exams);
        }

        function onErrorResponse(response) {
            $scope.errors = response.data;
        }

        function onError(response) {
            notifySrv.error(response.data);
        }

        function onSuccess() {
            notifySrv.success('common.infoSaved');
        }

        function mapIds(array) {
            return array ? array.map(function(a) {
                return a.id
            }) : [];
        }

        function openDatePicker($event, key) {
            $event.preventDefault();
            $event.stopPropagation();

            closeAllDatePickers(key);

            $timeout(function() {
                $event.target.focus();
            }, 100);
        }

        function closeAllDatePickers(datePicker) {
            $scope.datePickers.employmentDate = false;

            $scope.datePickers[datePicker] = true;
        }
    }
})();
