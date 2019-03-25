(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization')
        .controller('customizationController', customizationController);

    customizationController.$inject = [
        '$rootScope',
        'authService',
        'customizationNavigationFactory'
    ];    

    function customizationController($rootScope, authService, customizationNavigationFactory) {
    	/* jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'customization.customizationPanelHeader';

        vm.customizationMenuItems = customizationNavigationFactory.getCustomizationMenuItems();
        vm.hasAccessToCustomization = authService.hasOneOfPermissions([
            'EVENT_ADMINISTRATION',
            'SERVICEREQUESTS_ADMINISTRATION',
            'ORGANIZATION_ADMINISTRATION',
            'JOB_ADMINISTRATION',
            'KUDOSSHOP_ADMINISTRATION',
            'KUDOS_ADMINISTRATION',
            'EXTERNALLINK_ADMINISTRATION'
        ]);
    }

})();
