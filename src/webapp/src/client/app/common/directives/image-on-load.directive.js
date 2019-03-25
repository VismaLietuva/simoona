(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .directive('imageOnLoad', imageOnLoad);

    function imageOnLoad() {
        return {
            restrict: 'A',
            scope:{
                imageOnLoad: '&',
                imageOnLoadData: '='
            },
            link: function (scope, element, attrs) {
                element.bind('load', function () {
                    var _this = this;

                    scope.$apply(function () {
                        if(scope.imageOnLoadData){
                            scope.imageOnLoad({width: _this.width, height: _this.height, data: scope.imageOnLoadData});
                        }
                        else {
                            scope.imageOnLoad({ width: _this.width, height: _this.height });
                        }
                    });
                });
            }
        };
    }
})();