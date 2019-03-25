(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('noEnter', noEnter);

    function noEnter() {
        var directive = {
            restrict: 'A',
            link: linkFunc
        };
        return directive;
    }

    function linkFunc(scope, element) {
        element.keypress(function(e) {
           if (e.keyCode === 13) {
               e.preventDefault();
               return;
           }
       });
    }
})();
