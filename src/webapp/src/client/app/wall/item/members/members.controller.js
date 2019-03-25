(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('wallMembersController', wallMembersController);

    wallMembersController.$inject = [
        '$scope',
        '$timeout',
        '$stateParams',
        'authService',
        'wallRepository',
        'wallService',
        'errorHandler',
        'lodash'
    ];

    function wallMembersController($scope, $timeout, $stateParams, authService, wallRepository,
        wallService, errorHandler, lodash) {
        /*jshint validthis: true */
        var vm = this;

        vm.isWallAdmin = authService.hasPermissions(['POST_ADMINISTRATION']);
        vm.isWallModerator = false;
        vm.isLoading = true;
        vm.pageSize = 30;
        vm.wallServiceData = wallService.wallServiceData;
        vm.filter = {
            page: 1
        };
        vm.members = [];
        vm.filteredMembers = [];

        vm.expelMemberFromWall = expelMemberFromWall;
        vm.changePage = changePage;
        vm.onListFilter = onListFilter;
        vm.Math = Math;

        init();

        /////////

        function init() {
            wallRepository.getWallMembers($stateParams.wall).then(function (response) {
                vm.isLoading = false;
                vm.members = response;
                vm.wallServiceData.wallMembers = vm.members;
                vm.isWallModerator = !!lodash.find(response, { 'isModerator': true, 'isCurrentUser': true });
            }, errorHandler.handleErrorMessage);
        }

        function expelMemberFromWall(member) {
            if (!member.isLoading) {
                member.isLoading = true;
                wallRepository.expelMemberFromWall($stateParams.wall, member.id).then(function (response) {
                    lodash.remove(vm.members, {
                        id: member.id
                    });

                    $timeout(function() {
                        vm.wallServiceData.wallHeader.totalMembers--;
                        $scope.$apply();
                    }, 0);

                    if (member.isCurrentUser) {
                        wallService.removeWallFromList($stateParams.wall);
                        vm.wallServiceData.wallHeader.isFollowing = false;
                    }
                }, errorHandler.handleErrorMessage);
            }
        }

        function changePage(page) {
            vm.filter.page = page;
        }

        function onListFilter() {
            vm.filter.page = 1;
        }
    }
}());
