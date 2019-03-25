(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization')
        .factory('customizationNavigationFactory', customizationNavigationFactory);
    
    customizationNavigationFactory.$inject = [
        'authService',
        'lodash'
    ];

    function customizationNavigationFactory(authService, lodash) {

        var customizationMenuItems = [];

        var service = {
            defineCustomizationMenuItem: defineCustomizationMenuItem,
            getCustomizationMenuItems: getCustomizationMenuItems
        };
        return service;

        ////////

        function defineCustomizationMenuItem(item) {
            customizationMenuItems.push(item);
        }

        function getCustomizationMenuItems() {
            lodash.remove(customizationMenuItems, function(item){
                return !authService.hasPermissions([item.permission]);
            });
            return customizationMenuItems;
        }
    }
})();
