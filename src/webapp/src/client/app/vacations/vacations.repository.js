(function() {
    'use strict';

    angular
        .module('simoonaApp.Vacations')
        .factory('vacationsRepository', vacationsRepository);

    vacationsRepository.$inject = [
        '$resource',
        'shroomsFileUploader',
        'endPoint'
    ];

    function vacationsRepository($resource, shroomsFileUploader, endPoint) {
        var vacationsUrl = endPoint + '/Vacations/';

        var service = {
            getVacationHistory: getVacationHistory,
            getAvailableDays: getAvailableDays,
            uploadVacationFile: uploadVacationFile
        }

        return service;

        /////////

        function getVacationHistory() {
            return $resource(vacationsUrl + 'GetVacationHistory').query().$promise;
        }

        function getAvailableDays() {
            return $resource(vacationsUrl + 'AvailableDays').get().$promise;
        }

        function uploadVacationFile(files) {
            return shroomsFileUploader.upload(files, vacationsUrl + 'Upload');
        }
    }
})();
