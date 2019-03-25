(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('missingTranslationHandler', missingTranslationHandler);

    missingTranslationHandler.$inject = [
        '$log',
        'localStorageService'
    ];

    function missingTranslationHandler($log, localStorageService) {
        return function (translationId) {
            var authData = localStorageService.get('authorizationData');

            if (authData) {
                var orgString = authData.organizationName.toLowerCase();

                if (angular.isString(translationId) && translationId.indexOf(orgString) === -1) {
                    $log.warn('Translation for ' + translationId + ' doesn\'t exist');
                }
            }
        };
    }
})();
