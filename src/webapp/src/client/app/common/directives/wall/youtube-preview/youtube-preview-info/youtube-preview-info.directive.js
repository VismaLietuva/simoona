(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceYoutubePreviewInfo', youtubePreviewInfo);

    youtubePreviewInfo.$inject = [
        '$compile',
        'notifySrv',
        'youtubePreviewService'
    ];

    function youtubePreviewInfo($compile, notifySrv, youtubePreviewService) {

        var directive = {
            restrict: 'E',
            replace: true,
            template: '<div data-test-id="youtube-video-title" class="youtube-video-title"></div>',
            scope: {
                id: '@'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element) {
            /*jshint camelcase: false */
            youtubePreviewService.jsonp_query({
                id: scope.id
            }).$promise.then(function (data) {
                    scope.title = data.items[0].snippet.title;
                    var titleLink = angular.element('<a></a>').attr({
                        href: 'http://www.youtube.com/watch?v=' + scope.id,
                        target: '_blank'
                    });
                    var youtubeTitle = angular.element('<div></div>').toggleClass('youtube-title');
                    youtubeTitle.html('https://www.youtube.com');
                    element.empty();
                    titleLink.html(scope.title);
                    element.toggleClass('youtube-title-link');
                    element.append($compile(titleLink)(scope));
                    element.append($compile(youtubeTitle)(scope));
                },
                function (response) {
                    notifySrv.error(response);
                });
        }
    }
}());