(function() {
    'use strict';

    angular
        .module('simoonaApp.Settings')
        .factory('settingsRepository', settingsRepository);

    settingsRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function settingsRepository($resource, endPoint) {
        var userUrl = endPoint + '/user/';

        var service = {
            getGeneralSettings: getGeneralSettings,
            saveGeneralSettings: saveGeneralSettings,
            
            getNotifications: getNotifications,
            saveNotifications: saveNotifications,

            getUserLogins: getUserLogins,
            deleteUserLogin: deleteUserLogin,
        };
        return service;

        /////////

        //general
        function getGeneralSettings() {
            return $resource(userUrl + 'generalSettings').get().$promise;
        }

        function saveGeneralSettings(data) {
            return $resource(userUrl + 'generalSettings', '', {
                put: {
                    method: 'PUT'
                }
            }).put({ 
                languageCode: data.languageCode, 
                timeZoneId: data.timeZoneId 
            }).$promise;
        }

        //notifications
        function getNotifications() {
            return $resource(userUrl + 'notifications').get().$promise;
        }

        function saveNotifications(data) {
            var dtObj = {
                eventsAppNotifications : data.eventsAppNotifications,
                eventsEmailNotifications : data.eventsEmailNotifications,
                eventWeeklyReminderAppNotifications : data.eventWeeklyReminderAppNotifications,
                eventWeeklyReminderEmailNotifications : data.eventWeeklyReminderEmailNotifications,
                projectsAppNotifications : data.projectsAppNotifications,
                projectsEmailNotifications : data.projectsEmailNotifications,
                myPostsAppNotifications : data.myPostsAppNotifications,
                myPostsEmailNotifications : data.myPostsEmailNotifications,
                followingPostsAppNotifications : data.followingPostsAppNotifications,
                followingPostsEmailNotifications : data.followingPostsEmailNotifications
            }
            
            dtObj.walls = data.walls.map(function(wall){ 
                var dtObj = {};
                dtObj.wallId = wall.wallId;
                dtObj.IsAppNotificationEnabled = wall.isAppNotificationEnabled;
                dtObj.IsEmailNotificationEnabled = wall.isEmailNotificationEnabled;
                return dtObj;
            });

            return $resource(userUrl + 'notifications', '', {
                put: {
                    method: 'PUT'
                }
            }).put(data).$promise;
        }

        function getUserLogins() {
            return $resource(userUrl + 'Logins').query().$promise;
        }

        function deleteUserLogin(providerName) {
            return $resource(userUrl + 'DeleteLogin?providerName=' + providerName).delete().$promise;
        }
    }
})();