(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout.NavigationBar')
        .controller('navigationBarController', navigationBarController);

    navigationBarController.$inject = [
        '$state',
        '$uibModal',
        'authService',
        'wallService',
        'appConfig',
        'menuNavigationFactory',
        'impersonate',
        'Analytics',
        'leftMenuService',
        'notificationFactory'
    ];

    function navigationBarController($state, $uibModal, authService, wallService,
        appConfig, menuNavigationFactory, impersonate, Analytics, leftMenuService, notificationFactory) {
        /* jshint validthis: true */
        var vm = this;
        vm.topMenuItemsList = menuNavigationFactory.getTopMenuItems();
        vm.identity = authService.identity;
        vm.homeState = appConfig.homeStateName + '({wall: null, post: null, search: null})';
        vm.isAdmin = authService.identity.isAuthenticated && authService.isInRole('Admin');
        vm.isAuthenticated = authService.isAuthenticated;
        vm.isAuthenticatedNotNewUser = authService.isAuthenticatedNotNewUser;
        vm.isImpersonated = authService.identity.impersonated;
        vm.isImpersonationEnabled = impersonate;
        vm.openImpersonateModal = openImpersonateModal;
        vm.revertImpersonate = revertImpersonate;
        vm.search = search;
        vm.searchReset = searchReset;
        vm.reloadLeftMenuItems = reloadLeftMenuItems;
        vm.toggleSidebar = toggleSidebar;
        vm.isSearchVisible = false;
        vm.isPopupOpen = false;
        vm.closeNotificationsPopup = closeNotificationsPopup;

        vm.notifications = notificationFactory.notification;

        vm.showAdminButton = authService.hasOneOfPermissions([
            'APPLICATIONUSER_ADMINISTRATION',
            'ROLES_ADMINISTRATION',
            'OFFICE_ADMINISTRATION',
            'ROOMTYPE_ADMINISTRATION',
            'PROJECT_ADMINISTRATION',
            'EVENT_ADMINISTRATION',
            'SERVICEREQUESTS_ADMINISTRATION',
            'ORGANIZATION_ADMINISTRATION',
            'JOB_ADMINISTRATION',
            'KUDOSSHOP_ADMINISTRATION',
            'KUDOS_ADMINISTRATION',
            'EXTERNALLINK_ADMINISTRATION'
        ]);

        vm.showCustomizationLink = authService.hasOneOfPermissions([
            'EVENT_ADMINISTRATION',
            'SERVICEREQUESTS_ADMINISTRATION',
            'ORGANIZATION_ADMINISTRATION',
            'JOB_ADMINISTRATION',
            'KUDOSSHOP_ADMINISTRATION',
            'KUDOS_ADMINISTRATION',
            'EXTERNALLINK_ADMINISTRATION'
        ]);

        var orgName = authService.getOrganizationNameFromUrl();
        if (orgName){
            vm.showSimoonaSupport = orgName.toLowerCase() !== 'vismasuperlabs';
        }
        
        vm.searchQuery = {
            query: ''
        };

        init();

        ////////////

        function init() {
            if (authService.identity.isAuthenticated) {
                notificationFactory.init();
            }

            vm.searchQuery.query = $state.params.search;
        }

        function openImpersonateModal() {
            $uibModal.open({
                templateUrl: 'app/impersonate/impersonate.html',
                controller: 'impersonateController',
                controllerAs: 'vm'
            });
        }

        function revertImpersonate() {
            authService.revertImpersonate().then(function(response) {
                authService.getUserInfo(response.access_token).then(function(userInfo) {
                    authService.setAuthenticationData(userInfo, response.access_token);

                    authService.redirectToHome();
                });
            });
        }

        function search() {
            $state.go('Root.WithOrg.Client.Wall.Item.Feed', {
                search: vm.searchQuery.query,
                post: null,
                wall: null
            }, {
                reload: true
            });
        }

        function searchReset() {
            vm.searchQuery.query = null;

            $state.go('Root.WithOrg.Client.Wall.Item.Feed', {
                search: null,
                post: null
            }, {
                reload: true
            });
        }

        function reloadLeftMenuItems() {
            wallService.getChosenWallList(true);
        }

        function toggleSidebar() {
            var state = leftMenuService.getStatus();
            leftMenuService.setStatus(!state);
            document.getElementsByTagName('body')[0].classList.toggle('overflow');
        }
         
        function closeNotificationsPopup() {
            vm.isPopupOpen = false;
        }
        
    }

})();
