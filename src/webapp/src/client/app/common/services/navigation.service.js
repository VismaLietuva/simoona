(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('menuNavigationFactory', menuNavigationFactory);

    menuNavigationFactory.$inject = [
        'lodash'
    ];

    function menuNavigationFactory(lodash) {
        var leftMenuItems = [];
        var topMenuItems = [];
        var leftMenuList = null;

        var service = {
            leftMenuList: leftMenuList,

            defineLeftMenuItem: defineLeftMenuItem,
            getLeftMenuItems: getLeftMenuItems,
            defineTopMenuItem: defineTopMenuItem,
            getTopMenuItems: getTopMenuItems,
            makeLeftMenu: makeLeftMenu,
            deleteLeftMenuGroup: deleteLeftMenuGroup
        };
        return service;

        ////////

        function defineLeftMenuItem(item) {
            leftMenuItems.push(item);
        }

        function defineTopMenuItem(item) {
            topMenuItems.push(item);
        }

        function getLeftMenuItems() {
            return leftMenuItems;
        }

        function getTopMenuItems() {
            return topMenuItems;
        }

        function makeLeftMenu(groupList) {
            var menu = groupList;
            lodash.forEach(groupList, function(groupItem, key) {
                menu[key].menuItems = lodash.filter(leftMenuItems, function(menuItem) {
                    return menuItem.group === groupItem;
                });
            });

            service.leftMenuList = menu;
        }

        function deleteLeftMenuGroup(group) {
            leftMenuItems = lodash.remove(leftMenuItems, function(menuItem) {
                return menuItem.group !== group;
            });
        }
    }
})();
