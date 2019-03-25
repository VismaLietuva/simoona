(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('imageLoading', imageLoading);

    function imageLoading() {
        var directive = {
            restrict: 'A',
            scope: {
                imageLoading: '&'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element, attrs) {
            element.addClass('image-loading');

            element.bind('load', function() {
                scope.$apply(function () {
                    element.addClass('image-loaded');

                    if (attrs.imageLoading) {
                        scope.imageLoading();
                    }
                });
            });
        }
    }
})();
