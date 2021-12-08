(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .constant('ShowMoreSettings', {
            charactersCount: 1500
        })
        .directive('aceMessageShowMore', messageShowMore);

    messageShowMore.$inject = [
        '$compile',
        'wallService',
        'ShowMoreSettings'
    ];

    function messageShowMore($compile, wallService, showMoreSettings) {
        var directive = {
            restrict: 'A',
            templateUrl: 'app/common/directives/wall/message-show-more/message-show-more.html',
            scope: {
                message: '@',
                showMoreType: '@',
                hasHashtagify: '='
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element, attrs) {
            scope.showMore = showMore;
            scope.showLess = showLess;
            scope.tagTermClick = tagTermClick;

            scope.newMessage = '';

            var showMoreButton = '<a class="show-more-link" ng-click="showMore()" data-test-id="{{testIdShowMore}}">' +
                '{{"wall.showMore" | translate}}' +
                '</a>';

            var showLessButton = '<a class="show-less-link" ng-click="showLess()" data-test-id="{{testIdShowLess}}">' +
                '{{"wall.showLess" | translate}}' +
                '</a>';

            init();

            ////////////

            function init() {
                if (scope.showMoreType === 'post') {
                    scope.testIdShowMore = 'post-show-more';
                    scope.testIdShowLess = 'post-show-less';
                } else {
                    scope.testIdShowMore = 'comment-show-more';
                    scope.testIdShowLess = 'comment-show-less';
                }

                if (scope.message.length > showMoreSettings.charactersCount) {
                    scope.newMessage = getTruncatedMessage();
                    $compile(element.find('.show-more-container').html(showMoreButton))(scope);
                } else {
                    scope.newMessage = scope.message;
                }
            }

            function showMore() {
                element.find('.show-more-link').remove();
                scope.newMessage = scope.message;
                element.append(showLessButton);
                $compile(element.contents())(scope);
            }

            function showLess() {
                element.children('.show-less-link').remove();
                scope.newMessage = getTruncatedMessage();
                element.find('.show-more-container').html(showMoreButton);
                $compile(element.contents())(scope);
            }

            function tagTermClick(event) {
                var tagText = event.target.innerHTML;
                wallService.searchWall(tagText);
            }

            function getTruncatedMessage() {
                var message = scope.message.substring(0, showMoreSettings.charactersCount);

                if(message.match(/\*\*/gi).length % 2 !== 0) {
                    if(message[message.length - 1] === '*') {
                        return message + '*...';
                    }

                    return message + '**...';
                }

                return message + "...";
            }
        }
    }
})();
