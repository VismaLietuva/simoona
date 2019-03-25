(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('acePictureModal', acePictureModal);

    acePictureModal.$inject = [
        '$filter',
        'Lightbox'
    ];

    function acePictureModal($filter, Lightbox) {
        var directive = {
            priority: 1,
            restrict: 'A',
            link: function (scope, elem, attrs) {
                attrs.$observe('acePictureModal', function(acePictureModal) {
                    elem.bind('click', function () {
                        var image = $filter('picture')(scope.$eval(acePictureModal));
                        Lightbox.openModal([image], 0);
                    });
                });
            }
        };

        return directive;
    }
})();
