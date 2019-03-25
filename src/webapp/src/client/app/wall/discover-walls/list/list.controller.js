(function() {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('discoverWallsListController', discoverWallsListController);

    discoverWallsListController.$inject = [
        'wallMenuNavigationRepository',
        'wallService',
        'authService',
        'lodash',
        'errorHandler',
        'Analytics'
    ];

    function discoverWallsListController(wallMenuNavigationRepository, wallService, authService, lodash, 
        errorHandler, Analytics) {
        /*jshint validthis: true */
        var vm = this;
        var tempWallList = [];

        vm.wallList = [];
        vm.isWallListLoading = true;

        vm.toggleFollowWall = toggleFollowWall;
        vm.isCurrentUserWallOwner = wallService.isCurrentUserWallOwner;

        init();

        //////

        function init() {
            Analytics.trackEvent('Discover wall', 'discover walls', 'init');

            wallMenuNavigationRepository.listWalls('all').then(function(response) {
                vm.wallList = response;

                tempWallList = angular.copy(response);

                vm.isWallListLoading = false;
            }, errorHandler.handleErrorMessage);
        }

        function toggleFollowWall(wall) {
            wall.isLoading = true;
            wallMenuNavigationRepository.toggleFollowWall(wall.id).then(function() {
                wall.isFollowing = !wall.isFollowing;
                wall.isLoading = false;
                wall.isNewWall = true;

                if (wall.isFollowing) {
                    wallService.wallServiceData.wallList.push(wall);
                    wallService.sortFollowingWalls();
                } else {
                    wallService.removeWallFromList(wall.id);
                }

                Analytics.trackEvent('Discover wall', 'Follow ' + wall.isFollowing.toString(), wall.name);
            }, errorHandler.handleErrorMessage);
        }

        function isWallListChanged() {
            var wallList = angular.copy(vm.wallList).sort();
            var tempList = angular.copy(tempWallList).sort();

            return !!lodash.find(wallList, function(value, key) {
                return tempList[key].isFollowing !== value.isFollowing;
            });
        }
    }
}());
