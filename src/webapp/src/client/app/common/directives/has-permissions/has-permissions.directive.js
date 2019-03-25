(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('hasPermissions', hasPermissions);

    hasPermissions.$inject = ['authService'];

    function hasPermissions(authService) {
        var directive = {
            scope: {
                hasPermissions: '='
            },
            link: linkFunc,
            restrict: 'A'
        };
        return directive;

        function linkFunc(scope, element, attrs) {
            if (scope.hasPermissions && authService.hasPermissions(scope.hasPermissions)) {
                element.show();
            } else {
                element.hide();
            }
        }
    }

})();
