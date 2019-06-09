(function () {

    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('shroomsFileUploader', shroomsFileUploader);

    shroomsFileUploader.$inject = ['$http', 'localStorageService'];

    function shroomsFileUploader($http, localStorageService) {

        var service = {
            validate: validate,
            fileListToArray: fileListToArray,
            upload: upload,
            validateCustomFileType: validateCustomFileType
        };

        return service;

        function validate(files, config, callback) {
            var isFileListAccepted = true;

            if (!angular.isFunction(callback)) {
                var callback = function () { };
            }

            for (var i = 0; i < files.length; i++) {

                if (files[i].size > config.sizeLimit) {
                    callback('sizeError', i, files[i]);
                    isFileListAccepted = false;
                }
                if (config.allowed.indexOf(files[i].type) === -1) {
                    callback('typeError', i, files[i]);
                    isFileListAccepted = false;
                }
            }

            return isFileListAccepted;
        }

        function validateCustomFileType(files, config, callback) {
            var isFileListAccepted = true;

            if (!angular.isFunction(callback)) {
                var callback = function () { };
            }

            for (var i = 0; i < files.length; i++) {

                if (files[i].size > config.sizeLimit) {
                    callback('sizeError', i, files[i]);
                    isFileListAccepted = false;
                }
                var a = files[i].name.match(/\.([^\.]+)$/)[1];
                if (config.allowed.indexOf(files[i].name.match(/\.([^\.]+)$/)[1]) === -1) {
                    callback('typeError', i, files[i]);
                    isFileListAccepted = false;
                }
            }

            return isFileListAccepted;
        }

        function fileListToArray(fileList) {
            return Array.apply(null, fileList).map(function (file) { return file; });
        }

        function upload(files, uploadUrl) {
            var formData = new FormData();
            var authData = localStorageService.get('authorizationData');

            for (var i = 0; i < files.length; i++) {
                formData.append('file' + i, files[i], files[i].name);
            }

            return $http.post(uploadUrl, formData, {
                transformRequest: angular.identity,
                headers: {
                    'Content-Type': undefined,
                    'Organization': authData.organizationName,
                    'Authorization': 'Bearer ' + authData.token
                }
            });
        }
    }
})();
