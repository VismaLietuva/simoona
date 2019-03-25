(function(){
	
	'use strict';
	
    angular.module('simoonaApp.Common')
        .directive('lengthCounter', lengthCounter)

    
    function lengthCounter(){

    	var directive = { 
    		restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/length-counter/length-counter.html',
            scope: {
                currentLength : '=',
                maxLength : '='
            },
            controller: lengthCounterController,
            controllerAs: 'vm',
            bindToController: true,
    	}
    	return directive;	
    }

    function lengthCounterController(){
        var vm = this;

    }

}());


