(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('notificationHub', notificationHub);

    notificationHub.$inject = [
        '$rootScope',
        '$timeout',
        'authService',
        'endPoint',
        'wallService',
        'notificationFactory'
    ];

    function notificationHub($rootScope, $timeout, authService, endPoint, wallService, notificationFactory) {

        let connection = null;
        let allowConnection = false;

        const factory = {
            initHubConnection: initHubConnection,
            disconnectFromHub: disconnectFromHub
        };

        return factory;

        /////////

        function buildConnection() {
            var token = authService.identity.token;
            var org = authService.identity.organizationName;
            var url = endPoint + '/signalr?Organization=' + encodeURIComponent(org);

            var conn = new signalR.HubConnectionBuilder()
                .withUrl(url, { accessTokenFactory: function() { return token; } })
                .withAutomaticReconnect([0, 2000, 10000, 30000, 60000])
                .build();

            conn.on('newContent', function(wallId, wallType) {
                $rootScope.$apply(function() {
                    wallService.notifyAboutNewContentAvailable(wallId, wallType);
                });
            });

            conn.on('newNotification', function(notification) {
                notificationFactory.addNotification(notification);
            });

            conn.onclose(function() {
                if (allowConnection) {
                    $timeout(function() { startConnection(); }, 60000);
                }
            });

            return conn;
        }

        function startConnection() {
            connection.start().catch(function(err) {
                console.error('SignalR connection error: ' + err);
            });
        }

        function initHubConnection() {
            allowConnection = true;

            if (!connection) {
                connection = buildConnection();
                startConnection();
            } else if (connection.state === signalR.HubConnectionState.Disconnected) {
                startConnection();
            }
        }

        function disconnectFromHub() {
            if (connection) {
                allowConnection = false;
                connection.stop();
            }
        }
    }
})();
