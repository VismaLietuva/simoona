(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .constant('youtubeSettings', {
            width: 490,
            height: 315,
            previewWidth: 170,
            previewHeight: 110
        })
        .directive('aceYoutubePreview', youtubePreview);

    function youtubePreview() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/wall/youtube-preview/youtube-preview.html',
            scope: {
                id: '@',
                type: '@',
                playicon: '@',
                previewwidth: '@',
                previewheight: '@',
                playerwidth: '@',
                playerheight: '@'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element) {

            var frame = 0;
            var preview = '//img.youtube.com/vi/' + scope.id + '/' + frame + '.jpg';
            var playImage = angular.element('<img/>');

            element.css({
                width: scope.previewwidth,
                height: scope.previewheight,
                position: 'relative',
                background: 'url(' + preview + ') no-repeat center',
                backgroundSize: 'cover',
                cursor: 'pointer',
                marginTop: '10px'
            });

            if (scope.playicon) {
                playImage.attr({
                    alt: 'play',
                    title: 'Play',
                    src: scope.playicon
                });

                playImage.css({
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    width: '40px',
                    height: '40px;'
                });

                element.append(playImage);
            }

            element.bind('click', function () {
                element.css({
                    width: scope.playerwidth + 'px',
                    height: scope.playerheight + 'px'
                });
                var iframe = angular.element('<iframe></iframe>').attr({
                    width: '100%',
                    height: '100%',
                    frameborder: 0,
                    allowfullscreen: true,
                    src: '//www.youtube.com/embed/' + scope.id + '?rel=0&autoplay=1'
                });

                element.empty();
                element.append(iframe);
            });
        }
    }
})();
