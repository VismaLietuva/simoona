(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('notifySrv', notifySrv);

    notifySrv.$inject = ['toaster', '$translate'];

    function notifySrv(toaster, $translate) {
        var service = {
            success: success,
            error: error,
            warning : warning
        };

        return service;

        var timer = 5000;

        function success(title, message) {
            popAlert('success', title, message);
        }

        function error(title, message) {
            popAlert('error', title, message);
        }

        function warning(title, message) {
            popAlert('warning', title, message);
        }

        function popAlert(status, title, message) {
            title = $translate.instant(title);
            message = $translate.instant(message);
            
            if (angular.isArray(title)) {
                for (var i = 0; i < title.length; i++) {
                    if (angular.isString(title[i])) {
                        toaster.pop(status, title[i], '', timer);
                    }
                }
            } else {
                // Remove quotes
                if (typeof title === 'string' && title) {
                    title = title.replace(/^"(.+(?="$))"$/, '$1');
                }
                else {
                    title = '';
                }

                if (typeof message === 'string' && message) {
                    message = message.replace(/^"(.+(?="$))"$/, '$1');
                }
                else {
                    message = '';
                }
                toaster.pop(status, title, message, timer);
            }
        }
    }
})();