(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('notificationHub', notificationHub);

    notificationHub.$inject = [
        '$rootScope',
        'Hub',
        '$timeout',
        'authService',
        'endPoint',
        'wallService',
        'notificationFactory'
    ];

    function notificationHub($rootScope, Hub, $timeout, authService, endPoint, wallService, notificationFactory) {

        let hubConnection = null;

        let allowConnection = false;

        const hubConnectionStates = {
            connecting: 0,
            connected: 1,
            reconnecting: 2,
            disconnected: 4
        };

        const factory = {
            initHubConnection: initHubConnection,
            disconnectFromHub: disconnectFromHub
        };

        return factory;

        /////////

        function initHubConnection() {
            allowConnection = true;

            if (!hubConnection) {
                const hub = new Hub('Notification', {

                    rootPath: endPoint + '/signalr',

                    queryParams: {
                        'Organization': authService.identity.organizationName,
                        'Token': authService.identity.token
                    },
                    //client side methods
                    listeners: {
                        'newContent': function(wallId, wallType) {
                            $rootScope.$apply(function() {
                                wallService.notifyAboutNewContentAvailable(wallId, wallType);
                            });
                        },
                        'newNotification': function(notification) {
                            notificationFactory.addNotification(notification);
                        }
                    },

                    'stateChanged': function(state) {
                        if (state.newState === hubConnectionStates.disconnected) {
                            if (allowConnection) {
                                $timeout(function() {
                                    hubConnection.connect();
                                }, 60000);
                            }
                        }
                    }
                });

                hubConnection = hub;
            } else
                if (hubConnection.connection.state === hubConnectionStates.disconnected) {
                    hubConnection.connect();
                }
        }

        function disconnectFromHub() {
            if (hubConnection) {
                allowConnection = false;
                hubConnection.disconnect();
            }
        }
    }
})();
