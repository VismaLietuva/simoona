(function() {
    'use strict';

    angular
        .module('simoonaApp.Hashtags')
        .directive('hashtagify', hashtagify);

    hashtagify.$inject = ['$timeout', '$compile'];

    function hashtagify($timeout, $compile) {
        return {
            restrict: 'A',
            scope: {
                tagClick: '&hashtagify'
            },
            link: function(scope, element, attrs) {
                $timeout(function() {
                    var html = element.html();

                    if (html === '') {
                        return false;
                    }

                    if (attrs.hashtagify) {
                        var regex = XRegExp('(?:\\s|^|\<p\>)#([a-z0-9_\\p{L}]+)', 'g');

                        html = html.replace(regex, ' <a ng-click="tagClick({$event: $event})" class="hashtag">#$1</a>');
                    }
                    
                    element.html(html);

                    $compile(element.contents())(scope);
                }, 0);
            }
        };
    }
})();
