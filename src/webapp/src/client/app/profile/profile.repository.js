(function () {
    'use strict';

    angular
        .module('simoonaApp.Profile')
        .factory('profileRepository', profileRepository);

    profileRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function profileRepository($resource, endPoint) {
        var applicationUserUrl = endPoint + '/ApplicationUser/';
        var skillUrl = endPoint + '/Skill/';
        var qualificationLevelUrl = endPoint + '/QualificationLevel/';
        var certificateUrl = endPoint + '/Certificate/';
        var examUrl = endPoint + '/Exam/';
        var hashtagsUrl = endPoint + '/Hashtags/';
        var blacklistStateUrl = endPoint + '/Blacklist/';

        var service = {
            getUserProfile: getUserProfile,
            getDetails: getDetails,
            confirmUser: confirmUser,
            getUserProfilePersonal: getUserProfilePersonal,
            getUserProfileShrooms: getUserProfileShrooms,
            getUserProfileJob: getUserProfileJob,
            getUserProfileOffice: getUserProfileOffice,
            getProfile: getProfile,
            getProfilePersonal: getProfilePersonal,
            getProfileJob: getProfileJob,
            getPartTimeHours: getPartTimeHours,
            getProfileOffice: getProfileOffice,
            getProfileLogin: getProfileLogin,
            putPersonalInfo: putPersonalInfo,
            putJobInfo: putJobInfo,
            putOfficeInfo: putOfficeInfo,
            putShroomsInfo: putShroomsInfo,
            putUserCertificate: putUserCertificate,
            putCertificate: putCertificate,
            putExams: putExams,
            getAllHashtagsForAutoComplete: getAllHashtagsForAutoComplete,
            getImportantHashtags: getImportantHashtags,
            postImportantHashtags: postImportantHashtags,
            getUserForAutoComplete: getUserForAutoComplete,
            getProjectForAutoComplete: getProjectForAutoComplete,
            getJobTitleForAutoComplete: getJobTitleForAutoComplete,
            getQualificationLevelForAutoComplete: getQualificationLevelForAutoComplete,
            getCertificateForAutoComplete: getCertificateForAutoComplete,
            getExamForAutoComplete: getExamForAutoComplete,
            getExamNumbersForAutoComplete: getExamNumbersForAutoComplete,
            GetExamForAutoCompleteByTitleAndNumber: GetExamForAutoCompleteByTitleAndNumber,
            getSkillForAutoComplete: getSkillForAutoComplete,
            postSkill: postSkill,
            postCertificate: postCertificate,
            deleteCertificate: deleteCertificate,
            postExam: postExam,
            confirmRoomChange: confirmRoomChange,
            rejectRoomChange: rejectRoomChange,
            impersonate: impersonate,
            revertImpersonate: revertImpersonate,
            getBlacklistEntry: getBlacklistEntry,
            putBlacklistState: putBlacklistState,
            deleteBlacklistState: deleteBlacklistState,
            createBlacklistState: createBlacklistState,
            getBlacklistHistory: getBlacklistHistory
        };

        return service;

        function getUserProfile(sParams) {
            var url = applicationUserUrl + 'GetUserProfile/' + sParams.id;
            return $resource(url).get({}).$promise;
        }

        function getDetails(sParams) {
            var url = applicationUserUrl + 'GetDetails/' + sParams.id;
            return $resource(url).get({}).$promise;
        }

        function confirmUser(id) {
            return $resource(applicationUserUrl + 'ConfirmUser', { id: id }, { 'update': { method: 'PUT' } }).update().$promise;
        }

        function getUserProfilePersonal(userId) {
            var url = applicationUserUrl + 'GetUserProfile/' + userId + '/Personal';
            return $resource(url).get({}).$promise;
        }

        function getUserProfileShrooms(sParams) {
            var url = applicationUserUrl + 'GetUserProfile/' + sParams.id + '/Shrooms';
            return $resource(url).get({}).$promise;
        }

        function getUserProfileJob(userId) {
            var url = applicationUserUrl + 'GetUserProfile/' + userId + '/Job';
            return $resource(url).get({}).$promise;
        }

        function getUserProfileOffice(userId) {
            var url = applicationUserUrl + 'GetUserProfile/' + userId + '/Office';
            return $resource(url).get({}).$promise;
        }

        function getProfile() {
            return $resource(applicationUserUrl + 'GetProfile').get().$promise;
        }

        function getProfilePersonal() {
            return $resource(applicationUserUrl + 'GetProfile/Personal').get().$promise;
        }

        function getProfileJob() {
            return $resource(applicationUserUrl + 'GetProfile/Job').get().$promise;
        }

        function getPartTimeHours() {
            return $resource(applicationUserUrl + 'GetPartTimeHoursOptions', '', { 'query': { method: 'GET', isArray: true } }).query().$promise;
        }

        function getProfileOffice() {
            return $resource(applicationUserUrl + 'GetProfile/Office').get().$promise;
        }

        function getProfileLogin() {
            return $resource(applicationUserUrl + 'GetProfile/Login').get().$promise;
        }

        function putPersonalInfo(personalInfo, isConfirm) {
            if (personalInfo.birthDay !== null && typeof (personalInfo.birthDay) === 'object') {
                personalInfo.birthDay = personalInfo.birthDay.getFullYear() + '-' + (personalInfo.birthDay.getMonth() + 1) + '-' + personalInfo.birthDay.getDate();
            }

            return $resource(applicationUserUrl + 'PutPersonalInfo', null, { 'update': { method: 'PUT' }, params: { confirm: isConfirm } }).update(personalInfo).$promise;
        }

        function putJobInfo(jobInfo) {
            if (jobInfo.employmentDate !== null && typeof (jobInfo.employmentDate) === 'object') {
                jobInfo.employmentDate = jobInfo.employmentDate.getFullYear() + '-' + (jobInfo.employmentDate.getMonth() + 1) + '-' + jobInfo.employmentDate.getDate();
            }

            return $resource(applicationUserUrl + 'PutJobInfo', null, { 'update': { method: 'PUT' } }).update(jobInfo).$promise;
        }

        function putOfficeInfo(officeInfo) {
            return $resource(applicationUserUrl + 'PutOfficeInfo', null, { 'update': { method: 'PUT' } }).update(officeInfo).$promise;
        }

        function putShroomsInfo(shroomsInfo) {
            return $resource(applicationUserUrl + 'PutShroomsInfo', null, { 'update': { method: 'PUT' } }).update(shroomsInfo).$promise;
        }

        function putUserCertificate(jobInfo) {
            return $resource(applicationUserUrl + 'PutUserCertificate', null, { 'update': { method: 'PUT' } }).update(jobInfo).$promise;
        }

        function putCertificate(certificate) {
            return $resource(certificateUrl + 'Put', null, {
                'update': {
                    method: 'PUT'
                }
            }).update(certificate).$promise;
        }

        function putExams(params) {
            return $resource(applicationUserUrl + 'PutExams', null, { 'update': { method: 'PUT' } }).update(params).$promise;
        }

        function getAllHashtagsForAutoComplete(params) {
            return $resource(hashtagsUrl + 'GetAllHashtagsForAutoComplete').query({ hashtagText: params }).$promise;
        }

        function getImportantHashtags() {
            return $resource(hashtagsUrl + 'GetImportantHashtags').query().$promise;
        }

        function postImportantHashtags(model) {
            return $resource(hashtagsUrl + 'PostImportantHashtags').save(model).$promise;
        }

        function getUserForAutoComplete(params) {
            return $resource(applicationUserUrl + 'GetForAutoComplete').query(params).$promise;
        }

        function getProjectForAutoComplete(params) {
            return $resource(projectUrl + 'GetForAutoComplete').query(params).$promise;
        }

        function getJobTitleForAutoComplete(params) {
            return $resource(applicationUserUrl + 'GetJobTitleForAutoComplete').query(params).$promise;
        }

        function getQualificationLevelForAutoComplete(params) {
            return $resource(qualificationLevelUrl + 'GetForAutoComplete').query(params).$promise;
        }

        function getCertificateForAutoComplete(params) {
            return $resource(certificateUrl + 'GetForAutoComplete').query(params).$promise;
        }

        function getExamForAutoComplete(params) {
            return $resource(examUrl + 'GetExamForAutoComplete').query({ s: params }).$promise;
        }

        function getExamNumbersForAutoComplete(params) {
            return $resource(examUrl + 'GetExamNumbersForAutoComplete', '', {
                'query': {
                    method: 'GET',
                    isArray: true,
                    params: params
                }
            }).query(params).$promise;
        }

        function GetExamForAutoCompleteByTitleAndNumber(params) {
            return $resource(examUrl + 'GetExamForAutoCompleteByTitleAndNumber').query({
                title: params.title,
                number: params.number
            }).$promise;
        }

        function getSkillForAutoComplete(params) {
            return $resource(skillUrl + 'GetForAutoComplete').query(params).$promise;
        }

        function postSkill(model) {
            return $resource(skillUrl + 'Post').save(model).$promise;
        }

        function postCertificate(model) {
            return $resource(certificateUrl + 'Post').save(model).$promise;
        }

        function deleteCertificate(id) {
            return $resource(certificateUrl + 'Delete').delete({ id: id }).$promise;
        }

        function postExam(model) {
            return $resource(examUrl + 'Post', {}, { save: { method: 'POST', isArray: true } }).save(model).$promise;
        }

        function confirmRoomChange(applicationUserId) {
            var url = applicationUserUrl + 'ConfirmRoomChange';
            return $resource(url, { userId: applicationUserId }, { put: { method: 'PUT' } }).put().$promise;
        }

        function rejectRoomChange(applicationUserId) {
            var url = applicationUserUrl + 'RejectRoomChange';
            return $resource(url, { userId: applicationUserId }, { put: { method: 'PUT' } }).put().$promise;
        }

        function impersonate(username) {
            return $resource(applicationUserUrl + 'Impersonate', '', { get: { method: 'GET', params: { username: username } } }).get({ username: username }).$promise;
        }

        function revertImpersonate() {
            return $resource(applicationUserUrl + 'RevertImpersonate', '', { put: { method: 'PUT' } }).put().$promise;
        }

        function getBlacklistEntry(userId) {
            return $resource(blacklistStateUrl + 'Get').get({
                userId: userId
            }).$promise;
        }

        function putBlacklistState(params) {
            return $resource(blacklistStateUrl + 'Update', '', {
                put: {
                    method: 'PUT',
                    params: {
                        userId: params.userId,
                        endDate: params.endDate,
                        reason: params.reason
                    }
                }
            }).put().$promise;
        }

        function deleteBlacklistState(params) {
            return $resource(blacklistStateUrl + 'Cancel', '', {
                put: {
                    method: 'PUT',
                    params: {
                        userId: params.userId
                    }
                }
            }).put().$promise;
        }

        function createBlacklistState(params) {
            return $resource(blacklistStateUrl + 'Add').save({
                userId: params.userId,
                endDate: params.endDate,
                reason: params.reason
            }).$promise;
        }

        function getBlacklistHistory(params) {
            return $resource(blacklistStateUrl + 'History').query(params).$promise;
        }
    }
})();
