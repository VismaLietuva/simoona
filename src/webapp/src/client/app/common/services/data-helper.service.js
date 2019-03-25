(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('dataHelper', dataHelper);

    dataHelper.$inject = [];

    function dataHelper() {
        var service = {
            isIdParamValid: isIdParamValid
        };
        return service;

        /////////

        function isIdParamValid(id) {
            return !!id && /^\d+$/.test(id);
        }
    }

})();
