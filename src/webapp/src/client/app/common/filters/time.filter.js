(function () {
    'use strict';
    /**
     * @name time
     *
     * @description
     *      wrapper around date filter to work with time strings. Supports all date filter formats
     *  
     * @usage
     *      {{anyTimeProperty | time: 'HH:mm'}}
     *
     */
    angular.module('simoonaApp.Common')
        .filter('time', timeFilter);


    timeFilter.$inject = ['$filter'];
    function timeFilter($filter) {
        var dateFilterFn = $filter('date');

        return function (time, format, timezone) {
            var dateTime = time ? new Date('1970/01/01 ' + time) : time;
            var timeFormat = format || 'HH:mm:ss';
            return dateFilterFn(dateTime, timeFormat, timezone);
        }
    }
})();