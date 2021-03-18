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

            scope.$evalAsync(function() {
                let text = convertMarkdownToHtml(element[0].innerHTML);
                element[0].innerHTML = formatMentions(text);
            })

            function convertMarkdownToHtml(input) {
                const converterInput = input.replace('&gt;', '>'); // Sanitizer encodes this, needed for blockquote.
                return getMarkdownConverter().makeHtml(converterInput);
            }

            function getMarkdownConverter() {
                const converter = new showdown.Converter();
                changeSettings(converter);
                return converter;
            }

            function changeSettings(converter) {
                const enabledFeatures = [
                    'excludeTrailingPunctuationFromURLs',
                    'openLinksInNewWindow',
                    'requireSpaceBeforeHeadingText',
                    'simpleLineBreaks',
                    'simplifiedAutoLink',
                    'strikethrough',
                    'underline'
                ];
                enabledFeatures.forEach(option => converter.setOption(option, true));
            }

            function formatMentions(text) {
                var pattern = /\B@[\u00BF-\u1FFF\u2C00-\uD7FF\w]+/gi;
                var matches = text.match(pattern);
                
                if (matches) {
                    matches = matches.filter(onlyUnique);
                    
                    matches.forEach(function(cur) {
                        text = text.split(cur).join(`<strong>${cur}</strong>`);
                    });
                }

                return text;
            }

            function onlyUnique(value, index, self) { 
                return self.indexOf(value) === index;
            }
        }
    }
})();
