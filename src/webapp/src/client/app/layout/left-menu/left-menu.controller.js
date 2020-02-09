(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout.LeftMenu')
        .constant('leftMenuGroups', {
            activities: { resource: 'navbar.activities', iconClass: 'glyphicon glyphicon-activity' },
            company: { resource: 'navbar.company', iconClass: 'glyphicon glyphicon-compass' },
            externals: { resource: 'navbar.externals', iconClass: 'glyphicon glyphicon-new-window-alt' }
        })
        .controller('leftMenuController', leftMenuController);

    leftMenuController.$inject = [
        '$translate',
        'menuNavigationFactory',
        'externalLinksRepository',
        'leftMenuGroups',
        'authService',
        'wallService',
        'leftMenuService'
    ];

    function leftMenuController($translate, menuNavigationFactory, externalLinksRepository, leftMenuGroups,
        authService, wallService, leftMenuService) {
        /* jshint validthis: true */
        var vm = this;

        vm.isTranslating = true;
        vm.menuNavigationFactory = menuNavigationFactory;
        vm.wallData = wallService.wallServiceData;
        vm.isAuthenticated = authService.identity.isAuthenticated && !authService.isInRole('NewUser') && !authService.isInRole('External');
        vm.isAuthenticatedForWalls = authService.hasPermissions(['WALL_BASIC']);
        vm.sidebarOpen = true;
        vm.closeSidebar = closeSidebar;
        vm.overlayDismiss = overlayDismiss;
        vm.isSidebarOpen = isSidebarOpen;
        //vm.startUserWalkThrough = walkThroughService.startWalkThrough;

        init();

        ///////

        $translate(leftMenuGroups.externals.resource).then(function() {
            vm.isTranslating = false;
        });

        function init() {
            if (authService.hasPermissions(['EXTERNALLINK_BASIC'])) {
                vm.isLoading = true;
                externalLinksRepository.getExternalLinks().then(function(response) {
                    menuNavigationFactory.deleteLeftMenuGroup(leftMenuGroups.externals);

                    angular.forEach(response, defineMenuItem);

                    menuNavigationFactory.makeLeftMenu(leftMenuGroups);
                    vm.isLoading = false;
                    //vm.startUserWalkThrough();
                }, function() {
                    vm.isLoading = false;
                });
            } else {
                menuNavigationFactory.makeLeftMenu(leftMenuGroups);
                vm.isLoading = false;
                //vm.startUserWalkThrough();
            }
        }

        function defineMenuItem(item, index) {
            menuNavigationFactory.defineLeftMenuItem({
                url: item.url,
                permission: 'EXTERNALLINK_BASIC',
                name: item.name,
                order: index,
                group: leftMenuGroups.externals
            });
        }

        function overlayDismiss(e) {
            if (!!e.target.classList.contains('sidebar-overlay-container') && !!leftMenuService.getStatus()) {
                closeSidebar();
            }
        }

        function closeSidebar() {
            leftMenuService.setStatus(false);
            document.getElementsByTagName('body')[0].classList.toggle('overflow');
        }

        function isSidebarOpen() {
            return leftMenuService.getStatus();
        }
    }

})();
