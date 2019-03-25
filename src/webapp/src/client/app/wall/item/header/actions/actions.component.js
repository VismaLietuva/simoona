(function() {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .component('aceWallHeaderActions', {
            replace: true,
            bindings: {
                wall: '='
            },
            templateUrl: 'app/wall/item/header/actions/actions.html',
            controller: wallHeaderActionsController,
            controllerAs: 'vm'
        });
    
    wallHeaderActionsController.$inject = [
        'wallMenuNavigationRepository',
        'wallService',
        'authService',
        'errorHandler',
        'lodash',
        'Analytics'
    ];

    function wallHeaderActionsController(wallMenuNavigationRepository, wallService, authService,
        errorHandler, lodash, Analytics) {
        /*jshint validthis: true */
        var vm = this;

        vm.unfollow = false;

        vm.toggleFollowWall = toggleFollowWall;

        init();

        ///////

        function init() {
            vm.isLoading = false;
            vm.isWallOwner = wallService.isCurrentUserWallOwner(vm.wall.moderators);
            vm.hasWallEditPermissions = vm.isWallOwner || authService.hasPermissions(['POST_ADMINISTRATION']);
        }

        function toggleFollowWall(wall) {
            vm.isLoading = true;

            wallMenuNavigationRepository.toggleFollowWall(wall.id).then(function(response) {
                var currentWallMember = {
                    isModerator: false,
                    isCurrentUser: true,
                    fullName: response.firstName + ' ' + response.lastName,
                    id: response.id,
                    jobTitle: response.jobPosition,
                    profilePicture: response.pictureId
                };
                wall.isFollowing = !wall.isFollowing;
                vm.isLoading = false;

                if (wall.isFollowing) {
                    vm.unfollow = true;
                    wallService.wallServiceData.wallList.push(wall);
                    wallService.sortFollowingWalls();

                    wallService.wallServiceData.wallHeader.totalMembers++;
                    wallService.wallServiceData.wallMembers.push(currentWallMember);
                    wallService.wallServiceData.wallMembers.sort(function(a, b) {
                        if (a.fullName < b.fullName) {
                            return -1;
                        }

                        if (a.fullName > b.fullName) {
                            return 1;
                        }

                        return 0;
                    });
                } else {
                    wallService.removeWallFromList(wall.id);

                    wallService.wallServiceData.wallHeader.totalMembers--;
                    lodash.remove(wallService.wallServiceData.wallMembers, {
                        id: authService.identity.userId
                    });
                }

                Analytics.trackEvent('Wall header', 'Follow ' + vm.wall.isFollowing.toString(), vm.wall.name);
            }, errorHandler.handleErrorMessage);
        }
    }
}());
