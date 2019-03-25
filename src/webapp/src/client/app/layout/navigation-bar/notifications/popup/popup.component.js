(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout.NavigationBar')
        .component('aceNotificationsPopup', {
            bindings: {},
            replace: true,
            templateUrl: 'app/layout/navigation-bar/notifications/popup/popup.html',
            controller: notificationsPopupController,
            controllerAs: 'vm'
        });

    notificationsPopupController.$inject = ['notificationFactory'];

    function notificationsPopupController(notificationFactory) {
        /* jshint validthis: true */
        var vm = this;

        vm.notifications = notificationFactory.notification;
    }
})();
