(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceYoutubePreviewLink', youtubePreviewLink);

    youtubePreviewLink.$inject = [
        '$compile'
    ];

    function youtubePreviewLink($compile) {
        var directive = {
            restrict: 'E',
            replace: true,
            scope: {
                message: '@',
                type: '@',
                previewWidth: '@',
                previewHeight: '@',
                playerWidth: '@',
                playerHeight: '@'
            },
            link: linkFunc
        };
        return directive;

        function getYoutubeIdFromUrl(url) {
            var regExp = /^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#\&\?]*).*/;
            var match = url.match(regExp);

            if (match && match[7].length === 11) {
                return match[7];
            }

            return null;
        }

        function linkFunc(scope, element) {
            /*jslint maxlen: 200 */
            var regExp = /https?:\/\/(?:[0-9A-Z-]+\.)?(?:youtu\.be\/|youtube(?:-nocookie)?\.com\S*[^\w\s-])([\w-]{11})(?=[^\w-]|$)(?![?=&+%\w.-]*(?:['"][^<>]*>|<\/a>))[?=&+%\w.-]*/ig;
            var match = scope.message.match(regExp);

            if (match) {
                element.empty();

                for (var i = 0; i < match.length; i++) {
                    var youtubeId = getYoutubeIdFromUrl(match[i]);

                    if (youtubeId) {
                        appendYoutubeElements(youtubeId);
                    }
                }
            }

            function appendYoutubeElements(youtubeId) {
                var youtubeVideoInfo = angular.element('<ace-youtube-preview-info></ace-youtube-preview-info>').attr({
                    id: youtubeId
                });

                var youtubeLink = angular.element('<ace-youtube-preview></ace-youtube-preview>').attr({
                    id: youtubeId,
                    type: scope.type,
                    previewwidth: scope.previewWidth,
                    previewheight: scope.previewHeight,
                    playerwidth: scope.playerWidth,
                    playerheight: scope.playerHeight,
                    playicon: 'images/Youtube-icon-full_color.png'
                });
                element.append($compile(youtubeVideoInfo)(scope));
                element.append($compile(youtubeLink)(scope));
            }
        }
    }
})();
