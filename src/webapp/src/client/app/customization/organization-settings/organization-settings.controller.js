(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.OrganizationSettings')
        .controller('organizationSettingsController', organizationSettingsController);

    organizationSettingsController.$inject = [
        '$rootScope',
        '$state',
        'organizationSettingsRepository',
        'notifySrv',
        'errorHandler'
    ];

    function organizationSettingsController($rootScope, $state, organizationSettingsRepository, notifySrv, errorHandler) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'customization.organizationSettings';

        vm.saveSettings = saveSettings;
        vm.cancelUpdate = cancelUpdate;
        vm.getManagers = getManagers;

        vm.settings = {
            manager: null
        };
        vm.isLoading = true;

        init();

        /////////

        function init() {
            organizationSettingsRepository.getOrganizationSettings().then(function(response) {
                if (!!response && !!response.userId) {
                    vm.settings.manager = response;
                    vm.settings.manager.id = response.userId;
                }
                vm.isLoading = false;
            }, errorHandler.handleErrorMessage);
        }

        function saveSettings() {
            organizationSettingsRepository.postOrganizationSettings(vm.settings.manager).then(function(response) {
                notifySrv.success('common.successfullySaved');
                $state.go('Root.WithOrg.Admin.Customization.List');
            }, errorHandler.handleErrorMessage);
        }

        function getManagers(searchString) {
            return organizationSettingsRepository.getManagersForAutocomplete(searchString);
        }

        function cancelUpdate() {
            $state.go('Root.WithOrg.Admin.Customization.List');
        }
    }
})();
