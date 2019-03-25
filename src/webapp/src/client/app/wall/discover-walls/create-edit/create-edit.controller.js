(function() {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('discoverWallsCreateController', discoverWallsCreateController);

    discoverWallsCreateController.$inject = [
        '$rootScope',
        '$state',
        'wallMenuNavigationRepository',
        'pictureRepository',
        'wallRepository',
        'wallService',
        'authService',
        'wallSettings',
        'dataHandler',
        'notifySrv',
        'lodash',
        'errorHandler',
        'userRepository'
    ];

    function discoverWallsCreateController($rootScope, $state, wallMenuNavigationRepository, pictureRepository, wallRepository,
        wallService, authService, wallSettings, dataHandler, notifySrv, lodash, errorHandler, userRepository) {
        /*jshint validthis: true */
        var vm = this;

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Client.Wall.Create'),
            isEdit: $state.includes('Root.WithOrg.Client.Wall.Edit')
        };

        $rootScope.pageTitle = vm.states.isCreate ? 'wall.createNewWall' : 'wall.editWall';

        vm.userId = $state.params.userId || authService.identity.userId;
        vm.userFullName = authService.identity.fullName;

        vm.wallLogoSize = {
            w: wallSettings.wallLogoWidth,
            h: wallSettings.wallLogoHeight
        };
        vm.isSaveButtonEnabled = true;
        vm.wall = {
            moderators: []
        }
        vm.wallLogo = '';
        vm.wallCroppedLogo = '';
        vm.wallSettings = wallSettings;
        vm.saveWall = saveWall;
        vm.createWall = createWall;
        vm.editWall = editWall;
        vm.deleteWall = deleteWall;
        vm.searchUsersAsModerators = searchUsersAsModerators;

        init();

        ////////

        function init() {

            if (!!$state.params.id) {
                wallRepository.getWallDetails($state.params.id).then(function(response) {
                        vm.wall = response;
                        var hasWallEditPermissions = wallService.isCurrentUserWallOwner(response.moderators) || authService.hasPermissions(['POST_ADMINISTRATION']);
                        if (!hasWallEditPermissions) {
                            returnToMainWall(vm.wall.id);
                        }
                    },
                    function(error) {
                        errorHandler.handleErrorMessage(error);

                        returnToMainWall(null);
                    });
            }

            if (vm.states.isCreate) {
                vm.wall.moderators.push({ id: vm.userId, fullName: vm.userFullName });
            }
        }

        function saveWall(method, isImageChanged) {
            if (isImageChanged) {
                var wallImageBlob = dataHandler.dataURItoBlob(vm.wallCroppedLogo, vm.wallLogo.type);

                wallImageBlob.lastModifiedDate = new Date();
                wallImageBlob.name = vm.wallLogo.name;
                var wallLogo = wallImageBlob;

                pictureRepository.upload([wallImageBlob]).then(function(result) {
                    method(result.data);
                });
            } else {
                method();
            }
        }

        function createWall(image) {
            if (vm.isSaveButtonEnabled) {
                vm.isSaveButtonEnabled = false;

                if (image) {
                    vm.wall.logo = image;
                }

                wallRepository.createWall(vm.wall).then(function(response) {
                        notifySrv.success('common.successfullySaved');
                        $state.go('Root.WithOrg.Client.Wall.List');

                        vm.wall.id = response.id;
                        wallService.wallServiceData.wallList.push(vm.wall);
                    },
                    function(error) {
                        vm.isSaveButtonEnabled = true;
                        errorHandler.handleErrorMessage(error);
                    });
            }
        }

        function editWall(image) {
            if (vm.isSaveButtonEnabled) {
                vm.isSaveButtonEnabled = false;

                if (image) {
                    vm.wall.logo = image;
                }

                wallRepository.editWall(vm.wall).then(function(response) {
                        var wall = lodash.find(wallService.wallServiceData.wallList, {
                            id: vm.wall.id
                        });
                        var wallIndex = lodash.indexOf(wallService.wallServiceData.wallList, wall);

                        if (vm.wall.isFollowing) {
                            wallService.wallServiceData.wallList.splice(wallIndex, 1, vm.wall);
                        }

                        wallService.wallServiceData.isWallLoading = true;

                        wallService.getWallDetails(vm.wall.id, true);
                        notifySrv.success('common.successfullySaved');
                        returnToMainWall(vm.wall.id);
                    },
                    function(error) {
                        vm.isSaveButtonEnabled = true;
                        errorHandler.handleErrorMessage(error);
                    });
            }
        }

        function deleteWall(wall) {
            wallRepository.deleteWall(wall.id).then(function() {
                wallService.removeWallFromList(wall.id);
                if (wallService.wallServiceData.wallList.length < 2) {
                    returnToMainWall(wallService.wallServiceData.wallList[0].id);
                } else {
                    returnToMainWall(null);
                }
            }, errorHandler.handleErrorMessage);
        }

        function searchUsersAsModerators(search) {
            return userRepository.getForAutocomplete(search);
        }

        function returnToMainWall(id) {
            $state.go('Root.WithOrg.Client.Wall.Item.Feed', {
                search: null,
                post: null,
                wall: id
            });
        }
    }
}());
