(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceSliderToggleBtn', sliderToggleButton);

    function sliderToggleButton() {
        var directive = {
            templateUrl: 'app/common/directives/slider-toggle-btn/slider-toggle-btn.html',
            restrict: 'E',
            replace: true,
            scope: {
                value: '=',
                translation: '@'
            },
            controller: sliderToggleButtonController,
            controllerAs: 'vm',
            bindToController: true,
        }

        return directive;
    }

    function sliderToggleButtonController() {
        var vm = this;
    }
})();
