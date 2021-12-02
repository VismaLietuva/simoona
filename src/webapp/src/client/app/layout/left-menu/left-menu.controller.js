(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout.LeftMenu')
        .constant('leftMenuGroups', {
            activities: { resource: 'navbar.activities', iconClass: 'glyphicon glyphicon-activity' },
            company: { resource: 'navbar.company', iconClass: 'glyphicon glyphicon-compass' },
            externalsImportant: { resource: 'navbar.externalsImportant', iconClass: 'glyphicon glyphicon-new-window-alt' },
            externalsBasic: { resource: 'navbar.externalsBasic', iconClass: 'glyphicon glyphicon-new-window-alt' }
        })
        .controller('leftMenuController', leftMenuController);

    leftMenuController.$inject = [
        '$translate',
        '$q',
        'menuNavigationFactory',
        'externalLinksRepository',
        'leftMenuGroups',
        'authService',
        'wallService',
        'leftMenuService',
        'externalLinkTypes'
    ];

    function leftMenuController($translate, $q, menuNavigationFactory, externalLinksRepository, leftMenuGroups,
        authService, wallService, leftMenuService, externalLinkTypes) {
        /* jshint validthis: true */
        var vm = this;

        vm.isTranslating = true;
        vm.menuNavigationFactory = menuNavigationFactory;
        vm.wallData = wallService.wallServiceData;
        vm.isAuthenticated = authService.identity.isAuthenticated && !authService.isInRole('NewUser') && !authService.isInRole('External');
        vm.isAuthenticatedForWalls = authService.hasPermissions(['WALL_BASIC']);
        vm.sidebarOpen = true;
        vm.externalLinkTypes = externalLinkTypes;
        vm.closeSidebar = closeSidebar;
        vm.overlayDismiss = overlayDismiss;
        vm.isSidebarOpen = isSidebarOpen;
        //vm.startUserWalkThrough = walkThroughService.startWalkThrough;

        init();

        translateExternalLinks();

        function init() {
            if (authService.hasPermissions(['EXTERNALLINK_BASIC'])) {
                vm.isLoading = true;

                externalLinksRepository.getExternalLinks().then(function(response) {
                    menuNavigationFactory.deleteLeftMenuGroup(leftMenuGroups);

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

        function translateExternalLinks() {
            $q.all([
                $translate(leftMenuGroups.externalsBasic.resource),
                $translate(leftMenuGroups.externalsImportant.resource)
            ]).then(vm.isTranslating = false);
        }

        function defineMenuItem(item, index) {
            var linkToAdd = {
                url: item.url,
                permission: 'EXTERNALLINK_BASIC',
                name: item.name,
                order: index,
            }

            switch (item.type) {
                case vm.externalLinkTypes.Important.type:
                    if(!isUniqueExternalUrl(leftMenuGroups.externalsImportant.menuItems, linkToAdd)) {
                        break;
                    }
                    linkToAdd.group = leftMenuGroups.externalsImportant;
                    menuNavigationFactory.defineLeftMenuItem(linkToAdd);
                    break;

                case vm.externalLinkTypes.Basic.type:
                default:
                    if(!isUniqueExternalUrl(leftMenuGroups.externalsBasic.menuItems, linkToAdd)) {
                        break;
                    }
                    linkToAdd.group = leftMenuGroups.externalsBasic;
                    menuNavigationFactory.defineLeftMenuItem(linkToAdd);
                    break;
            }
        }

        function isUniqueExternalUrl(externals, newExternal) {
            if (externals == null) {
                return true;
            }

            for(const external of externals) {
                if(external.url === newExternal.url) {
                    return false;
                }
            }
            return true;
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
