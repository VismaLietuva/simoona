(function() {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .controller('KudosController', KudosController);

    KudosController.$inject = [
        '$rootScope',
        '$state',
        'authService'
    ];

    function KudosController($rootScope, $state, authService) {
        /*jshint validthis: true */
        var vm = this;

        vm.isActiveTab = isActiveTab;

        $rootScope.pageTitle = 'kudos.kudosTitle';

        vm.userId = $state.params.userId || authService.identity.userId;
        vm.hasKudosAdminPermissions = authService.hasPermissions(['KUDOS_ADMINISTRATION']);
        vm.currentTabState = $state.current.name;
        vm.currentOrganizationName = $state.params.organizationName;
        vm.kudosTabs = [{
            tabTitle: 'kudos.achievements',
            tabState: 'Root.WithOrg.Client.Kudos.KudosAchievementBoard',
            isTabShown: true
        }, {
            tabTitle: 'kudos.logs',
            tabState: 'Root.WithOrg.Client.Kudos.KudosUserInformation',
            isTabShown: true
        }, {
            tabTitle: 'kudos.admin',
            tabState: 'Root.WithOrg.Client.Kudos.KudosLogList',
            isTabShown: vm.hasKudosAdminPermissions
        }];

        init();

        //////////

        function init() {
            $rootScope.$on('$stateChangeStart', function(event, toState, toParams) {
                vm.userId = toParams.userId || vm.userId;
                vm.currentTabState = toState.name;
            });
        }

        function isActiveTab(state) {
            return vm.currentTabState === state;
        }
    }
})();
