(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceSoundcloudPlayer', soundcloudPlayer);

    function soundcloudPlayer() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/wall/soundcloud-player/soundcloud-player.html',
            scope: {
                url: '@',
                type: '@',
                playerheight: '@'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element) {
            element.css({
                height: '175px'
            });

            var iframe = angular.element('<iframe></iframe>').attr({
                width: '100%',
                height: '100%',
                frameborder: 0,
                allowfullscreen: true,
                src: 'https://w.soundcloud.com/player/?url=' + scope.url + '&auto_play=false&buying=false&liking=false&download=false&sharing=false&show_artwork=true&show_comments=false&show_playcount=false&show_user=true&hide_related=true&visual=false&start_track=0&callback=true'
            });

            element.empty();
            element.append(iframe);
        }
    }
})();
