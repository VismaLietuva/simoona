(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .directive('markdown', markdown);
        
    markdown.$inject = ['$filter'];

    function markdown($filter) {
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
                var convertedText = linkValidator(text, converter);
                convertedText = convertedText.toString().replace(/&lt;br&gt;/g, '<br>'); //changes all shown <br>'s with actual line break
                element[0].innerHTML = convertedText;
            })

            function changeSettings(converter){
                converter.setOption('strikethrough', true);
                converter.setOption('underline', true);
            }

            function linkValidator(text, converter){
                var reg = /(<a.*?<\/a>)/g;
                // text splits into array with elements and every second one is link which is not modified to markdowns and emojis
                var textAndLinks = text.split(reg);
                for (var i = 0; i < textAndLinks.length; i = i + 2){
                    textAndLinks[i] = converter.makeHtml(textAndLinks[i]);
                    textAndLinks[i] = $filter('imagify')(textAndLinks[i]);
                }
                var text = textAndLinks.join('');
                return text;
            }
        };
    }
})();
