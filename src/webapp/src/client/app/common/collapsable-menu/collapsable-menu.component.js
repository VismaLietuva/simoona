(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceCollapsableMenu', {
            replace: true,
            transclude: true,
            bindings: {
                menuName: '<',
                menuIconClass: '<',
                menuTextTranslation: '<',
                isCollapsed: '<'
            },
            templateUrl: 'app/common/collapsable-menu/collapsable-menu.html',
            controller: collapsableMenuController,
            controllerAs: 'vm'
        });

    function collapsableMenuController() {
        const vm = this;
        const localStorageName = vm.menuName + 'isCollapsed';
        vm.onCollapseClick = onCollapseClick;

        const wasCollapsed = localStorage.getItem(localStorageName);
        if (wasCollapsed !== 'true') {
            vm.isCollapsed = false;
        }

        function onCollapseClick() {
            vm.isCollapsed = !vm.isCollapsed;
            localStorage.setItem(localStorageName, vm.isCollapsed);
        }
    }
})();
