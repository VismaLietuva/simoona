(function() {
    'use strict';

    angular
        .module('simoonaApp.Settings')
        .controller('settingsNotificationsController', settingsNotificationsController);

    settingsNotificationsController.$inject = [
        'settingsRepository',
        'appConfig',
        'errorHandler',
        'notifySrv',
        'Analytics',
        'filterFilter'
    ];

    function settingsNotificationsController(settingsRepository, appConfig, errorHandler,
        notifySrv, Analytics, filterFilter) {
        /*jshint validthis: true */
        var vm = this;

        vm.isLoading = true;
        vm.settings = {};
        vm.appConfig = appConfig;
        vm.settings.eventsAppNotifications = {};
        vm.settings.eventsEmailNotifications = {};
        vm.settings.projectsAppNotifications = {};
        vm.settings.projectsEmailNotifications = {};
        vm.settings.myPostsAppNotifications = {};
        vm.settings.myPostsEmailNotifications = {};
        vm.settings.followingPostsAppNotifications = {};
        vm.settings.followingPostsEmailNotifications = {};
        vm.settings.mentionEmailNotifications = {};

        vm.save = saveNotifications;

        init();

        /////////

        function init() {
            settingsRepository.getNotifications().then(function(response) {
                if(response) {
                    vm.settings.mainWallName = filterFilter(response.walls, { isMainWall: true })[0].wallName;
                    vm.settings.walls = response.walls;
                    vm.settings.eventsAppNotifications = response.eventsAppNotifications;
                    vm.settings.eventsEmailNotifications = response.eventsEmailNotifications;
                    vm.settings.eventWeeklyReminderAppNotifications = response.eventWeeklyReminderAppNotifications;
                    vm.settings.eventWeeklyReminderEmailNotifications = response.eventWeeklyReminderEmailNotifications;
                    vm.settings.projectsAppNotifications = response.projectsAppNotifications;
                    vm.settings.projectsEmailNotifications = response.projectsEmailNotifications;
                    vm.settings.myPostsAppNotifications = response.myPostsAppNotifications;
                    vm.settings.myPostsEmailNotifications = response.myPostsEmailNotifications;
                    vm.settings.followingPostsAppNotifications = response.followingPostsAppNotifications;
                    vm.settings.followingPostsEmailNotifications = response.followingPostsEmailNotifications;
                    vm.settings.mentionEmailNotifications = response.mentionEmailNotifications;
                    vm.settings.createdLotteryEmailNotifications = response.createdLotteryEmailNotifications;
                }
                vm.isLoading = false;
            }, errorHandler.handleErrorMessage);
        }

        function saveNotifications() {
            settingsRepository.saveNotifications(vm.settings).then(function() {
                Analytics.trackEvent('Settings notifications', 'save', vm.settings);

                notifySrv.success('common.infoSaved');
            }, errorHandler.handleErrorMessage);
        }
    }
}());
