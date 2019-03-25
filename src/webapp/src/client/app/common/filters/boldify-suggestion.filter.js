(function () {

    'use strict';

    angular
        .module('simoonaApp.Common')
        .filter('boldifySuggestion', function () {

            function escapeRegExp(string) {
                return string.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
            }

            return function (input, part) {
                input = input ? input.toUpperCase() : '';
                part = part ? part.toUpperCase() : '';
                return input.replace(new RegExp(escapeRegExp(part), 'gi'), '<strong>' + part + '</strong>');
            }
        });
})();