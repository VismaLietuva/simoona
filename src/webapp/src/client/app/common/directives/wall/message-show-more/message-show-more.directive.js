(function () {
    "use strict";

    angular
        .module("simoonaApp.Common")
        .directive("aceMessageShowMore", messageShowMore);

    messageShowMore.$inject = [
        "$compile",
        "wallService",
        "$timeout",
        "$window",
    ];

    function messageShowMore($compile, wallService, $timeout, $window) {
        var directive = {
            restrict: "A",
            templateUrl:
                "app/common/directives/wall/message-show-more/message-show-more.html",
            scope: {
                message: "@",
                showMoreType: "@",
                hasHashtagify: "=",
            },
            link: linkFunc,
        };
        return directive;

        function linkFunc(scope, element) {
            scope.showMore = showMore;
            scope.showLess = showLess;
            scope.tagTermClick = tagTermClick;

            var isShowMoreLinkVisible = false;

            var showMoreLink =
                '<a class="show-more-link" ng-click="showMore()" data-test-id="{{testIdShowMore}}">' +
                '{{"wall.showMore" | translate}}' +
                "</a>";

            var showLessLink =
                '<a class="show-less-link" ng-click="showLess()" data-test-id="{{testIdShowLess}}">' +
                '{{"wall.showLess" | translate}}' +
                "</a>";

            init();

            ////////////

            function init() {
                // using $timeout, because it will execute functions after loading DOM elements
                $timeout(function () {
                    if (isEllipsisActive(element.find(".post-text-container")[0])) {
                        addShowMoreLink();
                    }

                    angular.element($window).bind("resize", onResize);

                    scope.$on("$destroy", cleanUp);
                });

                if (scope.showMoreType === "post") {
                    scope.testIdShowMore = "post-show-more";
                    scope.testIdShowLess = "post-show-less";
                } else {
                    scope.testIdShowMore = "comment-show-more";
                    scope.testIdShowLess = "comment-show-less";
                }
            }

            function showMore() {
                element.find(".show-more-link").remove();
                element
                    .find(".post-text-container")
                    .removeClass("show-more-message-container");
                element.find(".show-more-container").html(showLessLink);
                $compile(element.contents())(scope);
            }

            function showLess() {
                element.find(".show-less-link").remove();
                element
                    .find(".post-text-container")
                    .addClass("show-more-message-container");
                
                if(isEllipsisActive(element.find(".post-text-container")[0])) {
                    element.find(".show-more-container").html(showMoreLink);
                }

                $compile(element.contents())(scope);
            }

            function tagTermClick(event) {
                var tagText = event.target.innerHTML;
                wallService.searchWall(tagText);
            }

            function isEllipsisActive(element) {
                return element.scrollHeight > element.clientHeight;
            }

            function onResize() {
                var ellipsisActive = isEllipsisActive(element.find(".post-text-container")[0]);

                if (ellipsisActive && !isShowMoreLinkVisible) {
                    addShowMoreLink();
                    return;
                }

                if (!ellipsisActive && isShowMoreLinkVisible) {
                    removeShowMoreLink();
                }
            }

            function addShowMoreLink() {
                $compile(
                    element.find(".show-more-container").html(showMoreLink)
                )(scope);

                isShowMoreLinkVisible = true;
            }

            function removeShowMoreLink() {
                element.find(".show-more-link").remove();
                isShowMoreLinkVisible = false;
            }

            function cleanUp() {
                angular.element($window).off("resize", onResize);
            }
        }
    }
})();
