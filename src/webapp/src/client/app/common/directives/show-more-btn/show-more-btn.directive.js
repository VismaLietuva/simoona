(function() {
    'use strict';


    angular.module('simoonaApp.Common')
        .directive('showMoreBtn', showMoreBtn);

    function showMoreBtn() {
        var directive = {
            restrict: 'E',
            scope: {
                showArray: '=',
                showLength: '=',
                minLength: '='
            },
            templateUrl: 'app/common/directives/show-more-btn/show-more-btn.html',
            controller: showMoreController,
            controllerAs: 'vm',
            bindToController: true
        };
        return directive;

    }

    function showMoreController() {
        var vm = this;

        vm.isCollapsed = true;
        vm.isVisible = isVisible;
        vm.toggle = toggle;
        vm.showLength = vm.minLength;

        function isVisible() {
            return vm.showArray.length > vm.minLength;
        }

        function toggle() {
            vm.showLength = vm.isCollapsed ? vm.showArray.length : vm.minLength;
            vm.isCollapsed = !vm.isCollapsed;
        }
    }

})();
