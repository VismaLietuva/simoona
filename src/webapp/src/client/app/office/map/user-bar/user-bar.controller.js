(function() {
    'use strict';

    angular
        .module('simoonaApp.UserBar')
        .controller('usersBarController', usersBarController);

    usersBarController.$inject = [
        '$scope',
        '$q',
        '$rootScope',
        '$timeout',
        '$window',
        'userRepository',
        'profileHelper',
        'zoom'
    ];

    function usersBarController($scope, $q, $rootScope, $timeout, $window, userRepository, profileHelper, zoom) {

        var tooltipOffset = 50; // reserve some more space to fit tooltip on screen after scrolling

        var overlapSideType = {
            none: 0,
            left: 1, // item should be scrolled right to fit into the view
            right: 2 // item should be scrolled left
        };

        var scrollModeType = {
            full: 0, // scroll users bar to the most left
            slight: 1 // scroll only needed amount
        };

        var showMode = {
            byFloor: 0,
            byRoom: 1
        };

        $scope.needMore = true;
        $scope.currentMode = showMode.byFloor;
        $scope.currentItems = [];
        $scope.applicationUsersByRoom = [];
        $scope.applicationUsersAll = [];
        $scope.selectedItem = null;

        var defferedItemsUpdated = $q.defer();
        var onItemsUpdatedPromise = defferedItemsUpdated.promise;

        $scope.$on('onItemsUpdated', function(e, args) {
            if ($scope.currentMode === showMode.byFloor) {
                $scope.currentItems = $scope.applicationUsersAll;
            } else if ($scope.currentMode === showMode.byRoom) {
                $scope.currentItems = $scope.applicationUsersByRoom;
            }
        });

        $scope.onInfiniteScrollGet = function(countToFillSpace, itemsToAdd) {
            if (!$scope.params.roomId && !$scope.params.user && $scope.needMore) {
                onItemsUpdatedPromise.then(function () {
                    $scope.getItems(countToFillSpace, itemsToAdd);
                });
            }
        };

        $scope.loadData = function() {
            if ($scope.params.user) {
                if ($scope.params.applicationUser.roomId) {
                    $scope.getItemsByRoom($scope.params.applicationUser.roomId, $scope.params.applicationUser);
                } else {
                    $scope.applicationUsersByRoom.push($scope.params.applicationUser);
                    $scope.currentMode = showMode.byRoom;
                    $scope.$broadcast('onItemsUpdated', { highlightItem: $scope.params.applicationUser });
                }
            } else if ($scope.params.roomId) {
                $scope.getItemsByRoom($scope.params.roomId);
            } else if ($scope.params.floorId) {
                $scope.currentMode = showMode.byFloor;
                $scope.applicationUsersAll = [];
                $scope.needMore = true;
                $scope.getItems(0, 0);
                $scope.$broadcast('executeInfiniteScrollLoad');
                $scope.$broadcast('onItemsUpdated');
            }
            defferedItemsUpdated.resolve();
        };

        $scope.getItems = function (countToFillSpace, itemsToAdd) {
            $scope.options = {
                countToFillSpace: countToFillSpace,
                itemsToAdd: itemsToAdd
            };

            if (!$scope.needMore) {
                return;
            }

            var currentCount = $scope.applicationUsersAll.length;
            var filter = {};

            if (currentCount === 0) {
                filter.page = 1;

                var neededCount = Math.round(countToFillSpace) + itemsToAdd;
                var roundedToPageSize = Math.ceil(neededCount / itemsToAdd) * itemsToAdd;

                filter.pageSize = roundedToPageSize;
            } else {
                var currentPage = Math.floor(currentCount / itemsToAdd);

                if (filter.page === currentPage + 1) {
                    // If page is not changed - stop getting new data. 
                    //  It's needed when it tries to get data even after all data is recieved (scrollbar is at the end).
                    // Without this check it would try getting the data infinitely.
                    return;
                }

                filter.page = currentPage + 1;
                filter.pageSize = itemsToAdd;
            }

            filter.floorId = $scope.params.floorId;

            // TODO: This request might happen even when $scope.needMore is false, 
            //  because the previous $promise might be still not resolved so quick.
            userRepository.getPagedByFloor(filter).then(function(applicationUsers) {
                if (!$scope.needMore) {
                    return;
                }

                // If it returns smaller amount than was requested - 
                //  it means all items are loaded and further requests are not needed
                if (applicationUsers.pagedList.length < filter.pageSize) {
                    $scope.needMore = false;
                }

                // Merge but exclude duplicates to avoid any accidental duplicated requests
                angular.forEach(applicationUsers.pagedList, function(item, i) {
                    var tempArray = $.grep($scope.applicationUsersAll, function(e) {
                        return e.id === item.id;
                    });

                    if (tempArray.length === 0) {
                        $scope.applicationUsersAll.push(item);
                    }
                });

                $scope.currentMode = showMode.byFloor;
                $scope.$broadcast('onItemsUpdated');
            });
        };

        $scope.getItemsByRoom = function(roomId, highlightItem) {
            userRepository.getByRoom({ roomId: roomId }).then(function(usersByRoom) {
                if (usersByRoom.length === 0) {
                    $scope.isEmptyRoom = true;
                } else {
                    $scope.isEmptyRoom = false;
                }

                $scope.applicationUsersByRoom = usersByRoom;
                $scope.currentMode = showMode.byRoom;
                $scope.$broadcast('onItemsUpdated', { highlightItem: highlightItem });
            });
        };

        $scope.onBlockMouseOver = function(usersBarItem) {
            $scope.onBlockAction(usersBarItem, false);
        };

        $scope.onBlockClick = function(usersBarItem) {
            $scope.onBlockAction(usersBarItem, true);
        };

        $scope.onBlockAction = function(usersBarItem, doScroll) {
            if (!$scope.selectedItem || $scope.selectedItem.id !== usersBarItem.id) {
                var element = angular.element('#users-bar-' + usersBarItem.id);
                var overlapSide = $scope.getOverlapSide(element);
                var scrollMode = scrollModeType.slight;

                if (doScroll && overlapSide !== overlapSideType.none) {
                    $scope.usersBarScrollToItem(usersBarItem, false, overlapSide, scrollMode);
                }

                $scope.$emit('usersBarItemHiglighted', usersBarItem.roomId);

                $scope.selectedItem = usersBarItem;
            } else {
                $scope.selectedItem = null;
            }
        };

        $scope.selectUsersBarItem = function(usersBarItem, doHighlight) {
            var element = angular.element('#users-bar-' + usersBarItem.id);
            var overlapSide = $scope.getOverlapSide(element);
            var scrollMode = scrollModeType.full;

            if (overlapSide !== overlapSideType.none) {
                $scope.usersBarScrollToItem(usersBarItem, doHighlight, overlapSide, scrollMode);
            }

            if (doHighlight) {
                $scope.highlightItem(usersBarItem);
            }
        };

        $scope.revertToAllUsersMode = function() {
            $scope.currentMode = showMode.byFloor;
            $scope.isEmptyRoom = false;

            if ($scope.applicationUsersAll === 0) {
                $scope.needMore = true;
                $scope.currentItems = [];
                $scope.$broadcast('executeInfiniteScrollLoad');
            } else {
                $scope.$broadcast('onItemsUpdated');
            }
        };

        $scope.getOverlapSide = function(element) {
            var overlapSide = overlapSideType.none;
            var usersBarBlock;

            if (element.hasClass('users-bar-block')) {
                usersBarBlock = element;
            }
            else {
                var parent = element.parents('.users-bar-block');

                if (parent.length > 0) {
                    usersBarBlock = parent;
                }
            }

            if (usersBarBlock) {
                var usersBarContainer = element.parents('.container');

                var containerLeftEdge = usersBarContainer.scrollLeft();
                var containerRightEdge = containerLeftEdge + usersBarContainer.width();

                var usersBarBlockLeftEdge = usersBarBlock.position().left;
                var usersBarBlockRightEdge = usersBarBlockLeftEdge + usersBarBlock.width();

                if (usersBarBlockLeftEdge < containerLeftEdge) {
                    overlapSide = overlapSideType.left;
                }
                else if (usersBarBlockRightEdge > containerRightEdge) {
                    overlapSide = overlapSideType.right;
                }
            }

            return overlapSide;
        };

        $scope.scrollToDomElement = function(element, overlapSide, scrollMode) {
            if (element.length > 0) {
                if (!overlapSide) {
                    overlapSide = overlapSideType.none;
                }

                if (!scrollMode) {
                    scrollMode = scrollModeType.full;
                }

                if (overlapSide !== overlapSideType.none) {
                    if (scrollMode === scrollModeType.full) {
                        element.scrollParent().scrollLeft(element.position().left - 100);
                    } else if (scrollMode === scrollModeType.slight) {
                        var scrollLeft = element.scrollParent().scrollLeft();

                        if (overlapSide === overlapSideType.left) {
                            element.scrollParent().scrollLeft(scrollLeft - element.width() - tooltipOffset);
                        }
                        else {
                            element.scrollParent().scrollLeft(scrollLeft + element.width() + tooltipOffset);
                        }
                    }
                }
            }
        };

        $scope.usersBarScrollToItemById = function(id, doHighlight, overlapSide, scrollMode) {
            angular.forEach($scope.currentItems, function(usersBarItem) {
                if (usersBarItem.id === id) {
                    $scope.usersBarScrollToItem(usersBarItem, doHighlight, overlapSide, scrollMode);
                }
            });
        };

        $scope.usersBarScrollToItem = function(usersBarItem, doHighlight, overlapSide, scrollMode) {
            var element = angular.element('#users-bar-' + usersBarItem.id);
            $scope.scrollToDomElement(element, overlapSide, scrollMode);
        };

        $scope.highlightItem = function (usersBarItem) {
            var element = angular.element('#users-bar-' + usersBarItem.id);

            // Simulate ng-click to show tooltip ($timeout is needed to workaround $apply)
            $timeout(function() {
                element.find('.users-bar-block-img').triggerHandler('click');
            }, 0);
        };

        //#region Profile
        $scope.getTitleAndQualification = function(applicationUser) {
            return profileHelper.getTitleAndQualification(applicationUser);
        };

        $scope.getFullName = function(applicationUser) {
            return profileHelper.getFullName(applicationUser);
        };

        $scope.getProjectsCsv = function(projects) {
            return profileHelper.getProjectsCsv(projects);
        };
        //#endregion
    }
})();