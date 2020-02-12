(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('hasOneOfPermissions', hasOneOfPermissions);

    hasOneOfPermissions.$inject = ['authService'];

    function hasOneOfPermissions(authService) {
        var directive = {
            scope: {
                hasOneOfPermissions: '='
            },
            link: linkFunc,
            restrict: 'A'
        };
        return directive;

        function linkFunc(scope, element, attrs) {
            if (scope.hasOneOfPermissions && authService.hasOneOfPermissions(scope.hasOneOfPermissions)) {
                element.show();
            } else {
                element.hide();
            }
        }
    }

})();
