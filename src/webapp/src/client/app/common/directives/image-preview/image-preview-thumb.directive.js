(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('imagePreviewThumb', imagePreviewThumb);

    function imagePreviewThumb() {
        var directive = {
            templateUrl: 'app/common/directives/image-preview/image-preview-thumb.html',
            restrict: 'AE',
            replace: true,
            scope: {
                source: '=',
                sources: '='
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope) {
            scope.dismiss = function () {
                scope.sources.splice(0, 1);
                scope.source = '';
            };
        }
    }
})();