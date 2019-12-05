(function() {
    'use strict';

    angular
        .module('simoonaApp.Layout.NavigationBar')
        .constant('popupNotificationThumbSettings', {
            width: 45,
            height: 45,
            mode: 'crop'
        })
        .constant('notificationTypes', {
            NewEvent: 0,
            NewWall: 1,
            WallPost: 2,
            EventPost: 3,
            ProjectPost: 4,
            WallComment: 5,
            EventComment: 6,
            ProjectComment: 7,
            FollowingComment: 8,
            EventReminder: 9
        })
        .component('acePopupNotification', {
            replace: true,
            transclude: true,
            bindings: {
                notification: '='
            },
            templateUrl: 'app/layout/navigation-bar/notifications/popup/notification/notification.html',
            controller: popupNotificationController,
            controllerAs: 'vm'
        });

    popupNotificationController.$inject = [
        '$translate',
        '$state',
        'notificationFactory',
        'popupNotificationThumbSettings',
        'notificationTypes'
    ];

    function popupNotificationController($translate, $state, notificationFactory, popupNotificationThumbSettings, notificationTypes) {
        /* jshint validthis: true */
        var vm = this;

        vm.notificationThumbSettings = popupNotificationThumbSettings;
        vm.notificationType = '';

        vm.markAsRead = markAsRead;

        init();

        ////////

        function init() {
            formatName();
            setTypeMessage();
            setNotificationLink();
        }
        
        function isDeleteButton(eventTarget) {
            return angular.element(eventTarget).hasClass('popup-notification-delete-button') ? true : false;
        }

        function markAsRead(notificationId) {
            notificationFactory.markAsRead([notificationId].concat(vm.notification.stackedIds));
        }

        function setTypeMessage() {
            switch (vm.notification.type) {
                case notificationTypes.NewEvent:
                    vm.notificationType = $translate.instant('navbar.newEventNotification');
                break;
                case notificationTypes.NewWall:
                      vm.notificationType = $translate.instant('navbar.newWallNotification');
                break;
                case notificationTypes.WallPost:
                    vm.notificationType = $translate.instant('navbar.newWallPostNotification', { name: vm.notification.description });
                break;
                case notificationTypes.EventPost:
                    vm.notificationType = $translate.instant('navbar.newEventPostNotification', { name: vm.notification.description });
                break;
                case notificationTypes.ProjectPost:
                    vm.notificationType = $translate.instant('navbar.newProjectPostNotification', { name: vm.notification.description });
                break;
                case notificationTypes.WallComment:
                    vm.notificationType = $translate.instant('navbar.wallPostCommentNotification');
                break;
                case notificationTypes.EventComment:
                    vm.notificationType = $translate.instant('navbar.wallPostCommentNotification');
                break;
                case notificationTypes.ProjectComment:
                    vm.notificationType = $translate.instant('navbar.wallPostCommentNotification');
                break;
                case notificationTypes.FollowingComment:
                    vm.notificationType = $translate.instant('navbar.followingPostCommentNotification');
                break;
                case notificationTypes.EventReminder:
                    vm.notificationType = $translate.instant('navbar.eventReminderNotification', {name: vm.notification.description}); 
                    vm.notification.title = $translate.instant('navbar.eventReminderNotifcationName');
                break;
                default:
                    vm.notificationType = '';
                break;
            }
        }
        
        function setNotificationLink() {
            switch (vm.notification.type) {
                case notificationTypes.NewEvent:
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            $state.go("Root.WithOrg.Client.Events.EventContent", {id: vm.notification.sourceIds.eventId}, {reload: true});
                        }    
                    }
                break;
                case notificationTypes.NewWall: 
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            $state.go("Root.WithOrg.Client.Wall.Item.Feed", {wall: vm.notification.sourceIds.wallId}, {reload: true});  
                        }
                    }
                break;
                case notificationTypes.WallPost:
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            $state.go("Root.WithOrg.Client.Wall.Item.Feed", {post: vm.notification.sourceIds.postId}, {reload: true});  
                        }
                    }
                break;
                case notificationTypes.EventPost:
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            $state.go("Root.WithOrg.Client.Events.EventContent", {id: vm.notification.sourceIds.eventId, postNotification: vm.notification.sourceIds.postId}, {reload: true});  
                        }
                    }
                break;
                case notificationTypes.ProjectPost:
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            $state.go("Root.WithOrg.Client.Projects.ProjectContent", {id: vm.notification.sourceIds.projectId, postNotification: vm.notification.sourceIds.postId}, {reload: true});  
                        }
                    }
                break;
                case notificationTypes.WallComment:
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            $state.go("Root.WithOrg.Client.Wall.Item.Feed", {post: vm.notification.sourceIds.postId}, {reload: true});  
                        }
                    }
                break;
                case notificationTypes.EventComment:
                    vm.openNotification = function(event, notificationId) {
                    if(!isDeleteButton(event.target)) {
                        markAsRead(notificationId);
                        $state.go("Root.WithOrg.Client.Events.EventContent", {id: vm.notification.sourceIds.eventId, postNotification: vm.notification.sourceIds.postId}, {reload: true});  
                        }
                    }
                break;
                case notificationTypes.ProjectComment:
                    vm.openNotification = function(event, notificationId) {
                    if(!isDeleteButton(event.target)) {
                        markAsRead(notificationId);
                        $state.go("Root.WithOrg.Client.Projects.ProjectContent", {id: vm.notification.sourceIds.projectId, postNotification: vm.notification.sourceIds.postId}, {reload: true}); 
                        }
                    }
                break;
                case notificationTypes.FollowingComment:
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            if (vm.notification.sourceIds.postId != null){
                                $state.go("Root.WithOrg.Client.Wall.Item.Feed", {post: vm.notification.sourceIds.postId}, {reload: true});  
                            }
                            else if (vm.notification.sourceIds.eventId != null){
                                $state.go("Root.WithOrg.Client.Events.EventContent", {id: vm.notification.sourceIds.eventId, postNotification: vm.notification.sourceIds.postId}, {reload: true});
                            }
                            else if (vm.notification.sourceIds.projectId != null){
                                $state.go("Root.WithOrg.Client.Projects.ProjectContent", {id: vm.notification.sourceIds.projectId, postNotification: vm.notification.sourceIds.postId}, {reload: true});
                            }
                            
                        }
                    }
                break;
                case notificationTypes.EventReminder:
                    vm.openNotification = function(event, notificationId) {
                        if(!isDeleteButton(event.target)) {
                            markAsRead(notificationId);
                            $state.go("Root.WithOrg.Client.Events.List.Type", {reload: true}); 
                        }
                    }
                break;
                default:
                    vm.openNotification = function() {
                        return;
                    }
                break;
            }
        }

        function formatName(){
            if (vm.notification.others > 0){
                vm.notification.title = vm.notification.title + $translate.instant('navbar.othersCommented', { others: vm.notification.others });
            }
        }
    }
})();
