(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout.NavigationBar')
        .component('aceNotificationsDismissAll', {
            templateUrl: 'app/layout/navigation-bar/notifications/popup/dismiss-all-btn/dismiss-all-btn.html',
            controller: notificationsDismissAllController,
            controllerAs: 'vm'
        });

    notificationsDismissAllController.$inject = ['notificationFactory'];
    
    function notificationsDismissAllController(notificationFactory) {
        /* jshint validthis: true */
        var vm = this;
        
        vm.markAllAsRead = notificationFactory.markAllAsRead;
    }
})();
