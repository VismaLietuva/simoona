(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('timepickerConvertions', timepickerConvertions);

    timepickerConvertions.$inject = ['toaster'];

    function timepickerConvertions(toaster) {
        return {
            convertToString: function (time) {
                var date = new Date(time);
                return String.format("{0}:{1}:{2}", date.getHours(), date.getMinutes(), date.getSeconds());
            },
            covertToDateObject: function (timeString) {
                if (timeString)
                {
                    var time = timeString.split(':');
                    return new Date().setHours(time[0], time[1], time[2]);
                }
                return new Date().setHours(0, 0, 0);
            }
        };
    }
})();