(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('hasOneOfPermissions', hasOneOffPermissions);

    hasPermissions.$inject = ['authService'];

    function hasPermissions(authService) {
        var directive = {
            scope: {
                hasOneOffPermissions: '='
            },
            link: linkFunc,
            restrict: 'A'
        };
        return directive;

        function linkFunc(scope, element, attrs) {
            if (scope.hasOneOffPermissions && authService.hasOneOfPermissions(scope.hasOneOffPermissions)) {
                element.show();
            } else {
                element.hide();
            }
        }
    }

})();
