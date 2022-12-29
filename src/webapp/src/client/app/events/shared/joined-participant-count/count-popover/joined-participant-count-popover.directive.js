(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .directive(
            'aceJoinedParticipantCountPopover',
            JoinedParticipantCountPopover
        );

    JoinedParticipantCountPopover.$inject = [
        '$compile',
        '$templateCache'
    ];

    function JoinedParticipantCountPopover($compile, $templateCache) {
        var directive = {
            restrict: 'A',
            transclude: true,
            templateUrl:
                'app/events/shared/joined-participant-count/count-popover/joined-participant-count-popover.html',
            scope: {
                event: '=',
                showPopover: '='
            },
            link: linkFunc,
        };
        return directive;

        function linkFunc(scope, element) {
            registerPopover(element, scope);
            managePopoverState(scope, element);
        }

        function registerPopover(element, scope) {
            element.popover({
                html: true,
                content: getPopoverContent(scope),
                placement: 'bottom',
                trigger: 'hover'
            });
        }

        function managePopoverState(scope, element) {
            scope.$watch('showPopover', function (newState, _) {
                if (newState) {
                    element.popover('enable');
                } else {
                    element.popover('disable');
                }
            });
        }

        function getPopoverContent(scope) {
            return $compile($templateCache.get('joined-participant-count-popover.html'))(scope);
        }
    }
})();
