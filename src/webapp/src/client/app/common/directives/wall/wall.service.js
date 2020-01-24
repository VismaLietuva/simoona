(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .constant('wallSettings', {
            postMaxLength: 5000,
            wallNameLength: 35,
            wallDescriptionLength: 128,
            wallLogoHeight: 100,
            wallLogoWidth: 150
        })
        .constant('WallsType', {
            MyWalls: 1,
            AllWalls: 2
        })
        .constant('WallsCount', {
            Min: 1
        })
        .factory('wallService', wallService);

    wallService.$inject = [
        '$location',
        '$timeout',
        '$state',
        'authService',
        'wallMenuNavigationRepository',
        'wallSettings',
        'errorHandler',
        'wallPostRepository',
        'wallRepository',
        'lodash',
        'notifySrv',
        'appConfig',
        'SmoothScroll',
        'WallsType',
        'WallsCount'
    ];

    function wallService($location, $timeout, $state, authService, wallMenuNavigationRepository,
        wallSettings, errorHandler, wallPostRepository, wallRepository, lodash, notifySrv, appConfig, SmoothScroll, WallsType, WallsCount) {
        
        var wallServiceData = {
            posts: [],
            isScrollingEnabled: true,
            isWallLoading: true,
            isWallListLoading: true,
            isWallHeaderLoading: true,
            isWallPostsLoading: true,
            wallList: [],
            wallHeader: null,
            wallMembers: [],
            isNewContentAvailable: false,
            wallId: null,
            isWallModule: false
        };
        var busy = false;
        var settings = {
            page: 1
        };
        var tempWallId = null;

        var wallTypes = {
            Main: 0,
            UserCreated: 1,
            Events: 2
        };

        var service = {
            wallServiceData: wallServiceData,
            isCurrentUserWallOwner: isCurrentUserWallOwner,

            initWall: initWall,
            getCurrentWallId: getCurrentWallId,
            getChosenWallList: getChosenWallList,
            getWallDetails: getWallDetails,

            createPost: createPost,
            searchWall: searchWall,
            getPagedWall: getPagedWall,
            reloadWall: reloadWall,

            removePostFromList: removePostFromList,
            removeCommentFromList: removeCommentFromList,
            removeWallFromList: removeWallFromList,

            notifyAboutNewContentAvailable: notifyAboutNewContentAvailable,

            sortFollowingWalls: sortFollowingWalls
        };
        return service;

        ///////

        function initWall(isWallModule, wallId) {
            wallServiceData.wallId = wallId;
            wallServiceData.isWallModule = isWallModule;
            wallServiceData.isNewContentAvailable = false;
            wallServiceData.wallMembers = [];

            settings = {
                page: 1
            };
            wallId = wallId || $state.params.wall;

            if (!wallId) {
                tempWallId = null;
                wallServiceData.isWallLoading = true;
            } else if (!!wallId && tempWallId !== wallId.toString()) {
                wallServiceData.isWallLoading = true;
                tempWallId = wallId.toString();
            } else {
                wallServiceData.isWallPostsLoading = true;
            }

            if (!isWallModule) {
                settings.wallId = wallId;
                loadWallContent();
                return;
            }

            if (!wallServiceData.wallList.length) {
                getChosenWallList(isWallModule);
            } else {
                if (wallServiceData.wallList.length === WallsCount.Min) {
                    getWallDetails(wallId || wallServiceData.wallList[0].id);
                }

                setWallSettings(wallServiceData.wallList, isWallModule);
                disableNewWallBadge(wallServiceData.wallList, wallId);
            }
        }

        function disableNewWallBadge(wallList, wallId) {
            if (wallId) {
                var currentWall = lodash.find(wallList, function (wall) {
                    return wall.id.toString() === wallId.toString();
                });
                if (currentWall) {
                    currentWall.isNewWall = false;
                }
            }
        }

        function getChosenWallList(isWallModule) {
            wallMenuNavigationRepository.listWalls('followed').then(function (response) {
                var wallId = $state.params.wall || settings.wallId;
                if (!!wallId) {
                    wallId = wallId.toString();
                }

                if (isWallModule && response.length === 1 && (!wallId || response[0].id.toString() === wallId)) {
                    getWallDetails(response[0].id);
                }

                wallServiceData.wallList = response;
                wallServiceData.isWallListLoading = false;

                if (isWallModule && !settings.wallId) {
                    setWallSettings(response, isWallModule);
                }
            }, errorHandler.handleErrorMessage);
        }

        function getWallDetails(wallId) {
            if (!$state.includes(appConfig.homeStateName) && !$state.includes('Root.WithOrg.Client.Wall.Item.Members')) {
                wallServiceData.isWallHeaderLoading = false;
                return;
            }

            wallId = wallId || getCurrentWallId();
            wallServiceData.isWallHeaderLoading = true;

            wallRepository.getWallDetails(wallId).then(function (response) {
                wallServiceData.wallHeader = response;

                wallServiceData.isWallHeaderLoading = false;
            }, errorHandler.handleErrorMessage);
        }

        function setWallSettings(wallList, isWallModule) {
            var mainWall = lodash.find(wallList, function (wall) {
                return wall.type === 'Main';
            });

            if (!$state.params.wall && wallList.length > WallsCount.Min) {
                settings.wallId = null;
            } else if (isWallModule && !$state.params.wall && !$state.current.url.contains('/All') && wallList.length === WallsCount.Min) {
                if (!isValidPostId($state.params.post)) {
                    $location.search('wall', mainWall.id);
                }

                $state.params.wall = mainWall.id;
                settings.wallId = mainWall.id.toString();
            } else {
                settings.wallId = $state.params.wall;
            }

            loadWallContent();
        }

        function loadWallContent() {
            wallServiceData.posts = [];
            wallServiceData.isScrollingEnabled = true;
            busy = false;

            if (isValidPostId($state.params.post)) {
                getSinglePost($state.params.post);
            } else if (!!$state.params.search) {
                searchWall($state.params.search);
            } else {
                getPagedWall();
            }

            scrollTop();
        }

        function createPost(post, isWallModule) {
            wallPostRepository.createPost(post, settings.wallId).then(function () {
                initWall(isWallModule, settings.wallId);
            }, errorHandler.handleErrorMessage);
        }

        function isValidPostId(postId) {
            return !!postId && /^\d+$/.test(postId);
        }

        function searchWall(searchString) {
            wallServiceData.isWallLoading = true;
            angular.element('#wall-search-input').val(searchString);
            angular.element('.input-clear').removeClass('ng-hide');
            $location.search('search', searchString);
            $location.search('wall', null);

            settings = {
                page: 1,
                searchString: searchString
            };
            wallServiceData.isScrollingEnabled = true;

            wallPostRepository.searchWall(settings).then(function (response) {
                addPostsToWall(response, false);
                busy = false;
            });
        }

        function getPagedWall() {
            if (busy) {
                return;
            }

            busy = true;

            if (settings.searchString) {
                wallPostRepository.searchWall(settings).then(function (response) {
                    addPostsToWall(response, true);
                    busy = false;
                }, errorHandler.handleErrorMessage);
            } else {
                if (!settings.wallId) {
                    settings.wallsType = $state.current.url.contains('All') ? WallsType.AllWalls : WallsType.MyWalls;
                    wallPostRepository.getAllPosts(settings).then(function (response) {
                        addPostsToWall(response, true);
                        busy = false;
                    }, errorHandler.handleErrorMessage);
                } else {
                    wallPostRepository.getPosts(settings).then(function (response) {
                        addPostsToWall(response, true);
                        busy = false;
                        scrollToPostNotification();
                    }, function (error) {
                        if ($state.includes(appConfig.homeStateName) && !!$state.params.wall) {
                            redirectToHomeState();
                        } else {
                            errorHandler.handleErrorMessage(error);
                        }
                    });
                }
            }
        }

        function reloadWall(isWallModule) {
            initWall(isWallModule, settings.wallId);
        }

        function getSinglePost(postId) {
            if (busy) {
                return;
            }

            busy = true;

            wallPostRepository.getPost(postId).then(function (response) {
                wallServiceData.isScrollingEnabled = false;
                wallServiceData.isWallLoading = false;
                wallServiceData.isWallPostsLoading = false;
                addPostToWall(response);
                busy = false;
            }, function () {
                notifySrv.error('wall.postDoesNotExist');
                redirectToHomeState();
            });
        }

        function addPostsToWall(response, append) {
            var newPosts = response.pagedList;
            wallServiceData.isWallPostsLoading = false;

            if (newPosts.length && response.pageSize === newPosts.length) {
                settings.page++;
            }

            if (response.pageSize !== newPosts.length) {
                wallServiceData.isScrollingEnabled = false;
            }

            if (append) {
                newPosts = filterDuplicates(newPosts);
                wallServiceData.posts = wallServiceData.posts.concat(newPosts);
            } else {
                wallServiceData.posts = newPosts;
            }

            wallServiceData.isWallLoading = false;
        }

        function addPostToWall(response) {
            if (response.id) {
                wallServiceData.posts = [response];
            } else {
                wallServiceData.posts = [];
            }

            wallServiceData.isWallLoading = false;
        }

        function filterDuplicates(newPosts) {
            var existingIds = wallServiceData.posts.map(function (post) {
                return post.id;
            });
            var filteredPosts = newPosts.filter(function (post) {
                if (existingIds.indexOf(post.id) === -1) {
                    return post;
                }
            });
            return filteredPosts;
        }

        function removePostFromList(post) {
            var index = wallServiceData.posts.indexOf(post);
            wallServiceData.posts.splice(index, 1);
        }

        function removeCommentFromList(comment) {
            for (var i = 0; i < wallServiceData.posts.length; i++) {
                if (wallServiceData.posts[i].id === comment.postId) {
                    var index = wallServiceData.posts[i].comments.indexOf(comment);
                    wallServiceData.posts[i].comments.splice(index, 1);
                    break;
                }
            }
        }

        function redirectToHomeState() {
            wallServiceData.isWallLoading = false;
            $state.go(appConfig.homeStateName, {
                post: null,
                wall: null,
                search: null
            });

            initWall(true);
        }

        function scrollTop() {
            window.scrollTo(0, 0);
        }

        function getCurrentWallId() {
            return settings.wallId || $state.params.wall;
        }

        function isCurrentUserWallOwner(managers) {
            if (!managers.length) {
                return false;
            }

            return !!lodash.find(managers, function (value) {
                return value.id === authService.identity.userId;
            });
        }

        function removeWallFromList(wallId) {
            lodash.remove(wallServiceData.wallList, {
                id: parseInt(wallId)
            });
        }

        function notifyAboutNewContentAvailable(wallId, wallType) {
            if (wallServiceData.isWallModule && wallTypes.Events !== wallType) {
                notifyWallModule(wallId);
            } else {
                notifyEventModule(wallId);
            }
        }

        function notifyWallModule(wallId) {
            var isWallOpen = $state.params.wall && $state.params.wall.toString() === wallId.toString();

            if (!$state.params.wall || isWallOpen) {
                wallServiceData.isNewContentAvailable = true;
            }
        }

        function notifyEventModule(wallId) {
            if (wallId === wallServiceData.wallId) {
                wallServiceData.isNewContentAvailable = true;
            }
        }

        function sortFollowingWalls() {
            wallServiceData.wallList = lodash.orderBy(wallServiceData.wallList,
                function (wall) {
                    return [wall.type, wall.name.toLowerCase()];
                }, ['asc', 'asc']);
        }
        
        function scrollToPostNotification() {
            if (!!$state.params.postNotification) {
                $timeout(function () {
                    SmoothScroll.scrollTo($state.params.postNotification, 60);
                });
            }
        }
    }
}());
