(function() {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .component('aceDiscoverWallsMenuButton', {
            templateUrl: 'app/wall/discover-walls/menu-button/menu-button.html',
            controller: discoverWallsMenuButtonController,
            controllerAs: 'vm'
        });

    discoverWallsMenuButtonController.$inject = [
        'leftMenuService'
    ];

    function discoverWallsMenuButtonController(leftMenuService) {
        /* jshint validthis: true */
        var vm = this;

        vm.closeSidebar = closeSidebar;

        function closeSidebar() {
            leftMenuService.setStatus(false);
            document.getElementsByTagName('body')[0].classList.toggle('overflow');
        }
    }
})();
