(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .filter('timeSpan', timeSpanFilter);


    timeSpanFilter.$inject = ['$filter'];
    function timeSpanFilter($filter) {

        return function (timeSpan) {
        	if(timeSpan){
        		return timeSpan.substring(0, timeSpan.indexOf('.'));
        	}
        }
    }
})();