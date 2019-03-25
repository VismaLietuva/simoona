(function() {
    'use strict';

    angular
        .module('simoonaApp.Settings')
        .controller('settingsController', settingsController);

    settingsController.$inject = [
        '$rootScope'
    ];

    function settingsController($rootScope) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'settings.pageTitle';

        vm.tabs = [
            {
                resource: 'settings.general',
                state: 'Root.WithOrg.Client.Settings.General'
            },
            {
                resource: 'settings.notifications',
                state: 'Root.WithOrg.Client.Settings.Notifications'
            },
            {
                resource: 'settings.providers',
                state: 'Root.WithOrg.Client.Settings.Providers'
            }
        ];
    }
}());
