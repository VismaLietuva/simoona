(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceMessageLikeListPopover', messageLikeListPopover)
        .constant('popoverLikeCount', 10)
        .constant('popoverSwitchPositionValue', { y: 385 });

    messageLikeListPopover.$inject = [
        '$compile',
        '$window',
        '$templateCache',
        '$uibModal',
        'likeTypes',
        'popoverLikeCount',
        'popoverSwitchPositionValue',
        'authService'
    ];

    function messageLikeListPopover($compile, $window, $templateCache, $uibModal, likeTypes, popoverLikeCount, popoverSwitchPositionValue, authService) {
        var lastLikePopoverElement;
        var directive = {
            restrict: 'A',
            transclude: true,
            templateUrl: 'app/common/directives/wall/message-like/list/like-list-popover/like-list-popover.html',
            scope: {
                likes: '=',
                modalLikes: '=',
                modalLikesAll: '=',
                others: '=',
                popoverTitle: '@',
                popoverEmoji: '@',
                allLikeTypes: '@',
                currentModalLikeTab: '@'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, element) {
            element.bind('click', function () {
                $uibModal.open({
                    templateUrl: 'app/common/directives/wall/message-like/list/like-list-popover/like-list-popover-modal.html',
                    windowTopClass: 'popover-likes-modal',
                    controller: popoverModalController,
                    controllerAs: 'vm',
                    resolve : {
                        popoverScope: scope,
                    }
                });
            });

            scope.likeTypes = likeTypes;
            scope.popoverLikeCount = popoverLikeCount;
            scope.hiddenUserCount = (scope.likes.length - 1) - popoverLikeCount;
            scope.currentUser = authService.identity.userId;

            var html = $templateCache.get('messageLikeListPopoverTemplate.html');
            var popoverContent = $compile(html)(scope);

            element.popover({
                html: true,
                content: popoverContent,
                placement: getPopoverPlacement,
                trigger: 'hover'
            }).click(function (event) {
                angular.element(this).popover('hide');
                event.stopPropagation();
            });

            function getPopoverPlacement() {
                if(element[0].getBoundingClientRect().y > popoverSwitchPositionValue.y) {
                    return 'top';
                }

                return 'bottom';
            }
        }
    }

    popoverModalController.$inject = [
        '$uibModalInstance',
        'popoverScope',
        'likeTypes'
    ];

    function popoverModalController($uibModalInstance, popoverScope, likeTypes) {

        var vm = this;

        vm.modalLikes = popoverScope.modalLikes;
        vm.modalLikesAll = popoverScope.modalLikesAll;
        vm.currentLikeTab = popoverScope.currentModalLikeTab;
        vm.likeTypes = likeTypes;

        vm.showAllLikesTab = true;
        vm.dropdownOpen = false;
        vm.currentTabCount = 0;
        vm.firstRenderedTabIndex = -1;

        vm.setLikeTab = setLikeTab;
        vm.increaseTabCount = increaseTabCount;
        vm.getTabCount = getTabCount;
        vm.setFirstRenderedTabIndex = setFirstRenderedTabIndex;
        vm.closeModal = closeModal;
        vm.toggleDropdown = toggleDropdown;


        init();

        //////////

        function init() {
            if(!canShowAllLikesTab()) {
                vm.showAllLikesTab = false;
            }

            // By default show all likes
            if(vm.currentLikeTab === undefined) {
                vm.currentLikeTab = vm.likeTypes.length;
            }
        }

        function setLikeTab(tabIndex) {
            if(vm.dropdownOpen) {
                vm.dropdownOpen = false;
            }

            vm.currentLikeTab = tabIndex;
        }

        function canShowAllLikesTab() {
            var receivedLikes = vm.modalLikes.filter(function(likes) {
                return likes.length === 0;
            })

            return receivedLikes.length !== vm.likeTypes.length - 1;
        }

        function toggleDropdown() {
            vm.dropdownOpen = !vm.dropdownOpen;
        }

        function closeModal() {
            $uibModalInstance.close();
        }

        function increaseTabCount() {
            vm.currentTabCount++;
        }

        function getTabCount() {
            return vm.currentTabCount;
        }

        function setFirstRenderedTabIndex(index) {
            if(vm.firstRenderedTabIndex !== -1) {
                return;
            }

            vm.firstRenderedTabIndex = index;
        }
    }
}());
