(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('ngThumb', ngThumb);

    ngThumb.$inject = ['$window'];

    function ngThumb($window) {
        var helper = {
            support: !!($window.FileReader && $window.CanvasRenderingContext2D),
            isFile: function (item) {
                return angular.isObject(item) && item instanceof $window.File;
            },
            isImage: function (file) {
                return !!~[
                    'image/jpg',
                    'image/jpeg',
                    'image/png',
                    'image/bmp',
                    'image/gif']
                    .indexOf(file.type);
            }
        };

        var directive = {
            restrict: 'A',
            template: '<canvas/>',
            link: link
        };
        return directive;

        function link(scope, element, attributes) {
            if (!helper.support) return;

            var params = scope.$eval(attributes.ngThumb);

            if (!helper.isFile(params.file)) return;
            if (!helper.isImage(params.file)) return;

            var canvas = element.find('canvas');
            var reader = new FileReader();

            reader.onload = onLoadFile;
            reader.readAsDataURL(params.file);

            function onLoadFile(event) {
                var img = new Image();
                img.onload = onLoadImage;
                img.src = event.target.result;
            }

            function onLoadImage() {
                var width = params.width || this.width / this.height * params.height;
                var height = params.height || this.height / this.width * params.width;
                var classes = params.class || '';
                canvas.attr({ 'width': width, 'height': height, 'class': classes });
                canvas[0].getContext('2d').drawImage(this, 0, 0, width, height);
            }
        }
    }

})();