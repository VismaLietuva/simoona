(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('navbarHeightChangeWatcher', navbarHeightChangeWatcher);

    navbarHeightChangeWatcher.$inject = ['$timeout'];

    function navbarHeightChangeWatcher($timeout) {
        var directive = {
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, el) {
            
            $timeout(init,500);

            angular.element(window).on('resize', function () {
                init();
            });

            function init() {
                //Navigation bar fixed top new line break workaround
                $timeout(function () {
                    if (el.height() === 84) {
                        angular.element('body').removeClass('body-padding-top-42');
                        angular.element('body').addClass('body-padding-top-78');
                    } else {
                        angular.element('body').removeClass('body-padding-top-78');
                        angular.element('body').addClass('body-padding-top-42');
                    }
                });
            }

        }
    }
})();