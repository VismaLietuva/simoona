(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceExpandableMenu', {
            replace: true,
            transclude: true,
            bindings: {
                menuName: '<',
                menuIconClass: '<',
                menuTextTranslation: '<',
                isExpanded: '<'
            },
            templateUrl: 'app/common/expandable-menu/expandable-menu.html',
            controller: expandableMenuController,
            controllerAs: 'vm'
        });

    function expandableMenuController() {
        let vm = this;

        vm.onExpandClick = onExpandClick;
        function onExpandClick() {
            vm.isExpanded = !vm.isExpanded;
        }
    }
})();
