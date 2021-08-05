(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('notificationFactory', notificationFactory);

    notificationFactory.$inject = [
        'notificationRepository',
        'errorHandler',
        'lodash'
    ];

    function notificationFactory(notificationRepository, errorHandler, lodash) {
         const notification = {
            data: [],
            isBusy: false
        };

        const service = {
            notification: notification,

            init: init,
            addNotification: addNotification,
            markAsRead: markAsRead,
            markAllAsRead: markAllAsRead,
            anyUnreadNotifications: anyUnreadNotifications
        };

        return service;

        ////////

        function init()
        {
            notification.isBusy = true;
            notificationRepository.getNotifications().then(function (response){
                notification.data = response;
                notification.isBusy = false;
            }, errorHandler.handleErrorMessage);
        }

        function addNotification(item)
        {
            notification.data.unshift(item);
        }

        function markAsRead(notificationsIds)
        {
            notificationRepository.markAsRead(notificationsIds).then(function (response){
                lodash.remove(notification.data, function(value) {
                    for (let key in response) {
                        if (response[key] === value.id) {
                            return true;
                        }
                    }
                    return false;
                });
            }, errorHandler.handleErrorMessage);
        }

        function markAllAsRead()
        {
            notificationRepository.markAllAsRead().then(function (){
                notification.data = [];
            }, errorHandler.handleErrorMessage);
        }

        function anyUnreadNotifications()
        {
            return !notification.data.length;
        }

        function updateNotifications()
        {
            notificationRepository.getNotifications().then(function (response){
                const latestNotifications = response;

                const missingObjects = lodash.filter(response, function(obj) {
                    return !lodash.find(notification.data, obj);
                });

                lodash.each(missingObjects, function (obj) {
                    notification.data.push(obj);
                });

            }, errorHandler.handleErrorMessage);
        }
    }
})();
