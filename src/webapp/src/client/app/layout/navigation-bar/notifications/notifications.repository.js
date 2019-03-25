(function () {
    'use strict';

    angular.module('simoonaApp.Layout.NavigationBar')
        .factory('notificationRepository', notificationRepository);

    notificationRepository.$inject = ['$resource', 'endPoint'];

    function notificationRepository($resource, endPoint) {
        var notificationUrl = endPoint + '/Notification/';

        var service = {
            getNotifications: getNotifications,
            markAsRead: markAsRead,
            markAllAsRead: markAllAsRead
        };

        return service;

        ///////

        function getNotifications() {
            return $resource(notificationUrl + 'GetAll').query().$promise;
        }

        function markAsRead(notificationsIds) {
            return $resource(notificationUrl + 'MarkAsRead', {}, {
                put: {
                    method: 'PUT'
                }
            }).put(notificationsIds).$promise;
        }

        function markAllAsRead() {
            return $resource(notificationUrl + 'markAllAsRead', {}, {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }
    }
})();
