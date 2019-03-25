(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .directive('showMoreText', showMoreText);

    showMoreText.$inject = [];

    /**
     * @name showMoreText
     *
     * @description
     *      Folds/Unfolds text with show more/less button.
     *
     * @usage
     *      <any show-more-text show-more-text-text='prop' show-more-text-length="number"></any>
     *
     * @param {String} prop - property of type String
     * @param {Number} number - how many symbols should be displayed when collapsed
     */
    function showMoreText() {
        return {
            restrict: 'A',
            scope: {
                text: '=showMoreTextText',
                maxLength: '@showMoreTextLength'
            },
            templateUrl: 'app/common/directives/show-more-text/show-more-text.html',
            controller: ctrl
        };
    }

    ctrl.$inject = ['$scope'];
    function ctrl($scope) {
        $scope.collapsed = true;
        $scope.toggle = function () {
            $scope.collapsed = !$scope.collapsed;
        };

        splitText($scope, $scope.text, $scope.maxLength);

        $scope.$watch('text', function (newText) {
            splitText($scope, newText, $scope.maxLength);
        });
    }

    function splitText($scope, text, maxLength) {
        if (text && maxLength < text.length) {
            $scope.displayedText = String(text).slice(0, maxLength);
            $scope.toggleableText = String(text).slice(maxLength, text.length);
        } else {
            $scope.displayedText = text;
            $scope.toggleableText = null;
        }
    }

})();
