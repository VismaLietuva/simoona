(function() {
	"use strict";

    angular
    	.module('simoonaApp.Certificate')
    	.factory('certificateRepository', certificateRepository); 

	certificateRepository.$inject = ['$resource', 'endPoint'];

	function certificateRepository($resource, endPoint) {
        var baseUrl = endPoint + '/Certificate/';

        return {
            search: function(query) {
                return $resource(baseUrl + 'Search').query({ query: query });
            }
        }
    }
})();