(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceSoundcloudPlayerLink', soundcloudPlayerLink);

    soundcloudPlayerLink.$inject = [
        '$compile'
    ];

    function soundcloudPlayerLink($compile) {
        var directive = {
            restrict: 'E',
            replace: true,
            scope: {
                message: '@',
                type: '@'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element) {
            var regExp = /https?:\/\/?(?:soundcloud.com\S*[^\w\s-])[?=&+%\w.-]*/ig;
            var match = scope.message.match(regExp);

            if (match) {
                element.empty();

                for (var i = 0; i < match.length; i++) {
                    var url = match[i];

                    if (url) {
                        appendSoundcloudElement(url);
                    }
                }
            }

            function appendSoundcloudElement(url) {
                var soundcloudPlayer = angular.element('<ace-soundcloud-player></ace-soundcloud-player>').attr({
                    url: url,
                    type: scope.type
                });

                element.append($compile(soundcloudPlayer)(scope));
            }
        }
    }
})();
