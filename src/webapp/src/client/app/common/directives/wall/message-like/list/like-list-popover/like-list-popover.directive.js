(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceMessageLikeListPopover', messageLikeListPopover);

    messageLikeListPopover.$inject = [
        '$compile',
        '$window',
        '$templateCache'
    ];

    function messageLikeListPopover($compile, $window, $templateCache) {
        var lastLikePopoverElement;
        var directive = {
            restrict: 'A',
            transclude: true,
            templateUrl: 'app/common/directives/wall/message-like/list/like-list-popover/like-list-popover.html',
            scope: {
                likes: '=',
                popoverPlacement: '@'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element) {
            var html = $templateCache.get('messageLikeListPopoverTemplate.html');
            var popoverContent = $compile(html)(scope);

            element.popover({
                html: true,
                content: popoverContent,
                placement: scope.popoverPlacement,
                trigger: 'manual'
            }).click(function (event) {
                hideLastLikePopover(this);
                lastLikePopoverElement = element;
                angular.element(this).popover('toggle');
                event.stopPropagation();
            });

            $window.onclick = function () {
                hideLastLikePopover();
            };

            function hideLastLikePopover(newElement) {
                if (lastLikePopoverElement && lastLikePopoverElement !== newElement) {
                    angular.element(lastLikePopoverElement).popover('hide');
                }
            }
        }
    }
}());
