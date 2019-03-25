(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceMessageUserProfile', {
            restrict: 'E',
            bindings: {
                user: '=',
                date: '=',
                editedDate: '=',
                isEdited: '=',
                isHidden: '<'
            },
            templateUrl: 'app/common/directives/wall/user-profile/user-profile.html',
            controller: messageUserProfileController,
            controllerAs: 'vm'
        });

    function messageUserProfileController() {
        /*jshint validthis: true */
        var vm = this;
    }
}());
