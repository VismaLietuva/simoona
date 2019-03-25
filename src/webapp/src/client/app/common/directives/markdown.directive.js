(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .directive('markdown', markdown);

    function markdown() {
        var directive = {
            restrict: 'A',
            replace: true,
            link: linkFunc
        };
        return directive;
        function linkFunc (scope, element, attrs) {

            scope.$evalAsync(function(){
                var converter = new showdown.Converter();
                changeSettings(converter);
                var text = element[0].innerHTML;
                var convertedText = converter.makeHtml(text);
                convertedText = convertedText.toString().replace(/&lt;br&gt;/g, '<br>'); //changes all shown <br>'s with actual line break
                element[0].innerHTML = convertedText;
            })

            function changeSettings(converter){
                converter.setOption('emoji', true);
                converter.setOption('strikethrough', true);
                converter.setOption('underline', true);
            }
        };
    }
})();