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
        var vacationsUrl = endPoint + '/VacationPage/';

        var service = {
            getVacationContent: getVacationContent,
            setVacationContent: setVacationContent
        }

        return service;

        /////////

        function getVacationContent() {
            return $resource(vacationsUrl + 'Get', '', {
                'get' : {
                    method: 'GET'
                }
            }).get().$promise;
        }

        function setVacationContent(newContent) {
            return $resource(vacationsUrl + 'Edit', '', {
                'put' : {
                    method: 'PUT'
                }
            }).put({
                content : newContent
            }).$promise;
        }
    }
})();
