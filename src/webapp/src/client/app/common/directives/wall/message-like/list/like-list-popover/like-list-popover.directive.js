(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceMessageLikeListPopover', messageLikeListPopover)
        .constant('popoverLikesCount', 15);

    messageLikeListPopover.$inject = [
        '$compile',
        '$window',
        '$templateCache',
        '$uibModal',
        'likeTypes',
        'popoverLikesCount'
    ];

    function messageLikeListPopover($compile, $window, $templateCache, $uibModal, likeTypes, popoverLikesCount) {
        var lastLikePopoverElement;
        var directive = {
            restrict: 'A',
            transclude: true,
            templateUrl: 'app/common/directives/wall/message-like/list/like-list-popover/like-list-popover.html',
            scope: {
                likes: '=',
                modalLikes: '=',
                popoverPlacement: '@',
                popoverTitle: '@',
                popoverEmoji: '@',
                allLikeTypes: '@',
                modalLikesTab: '@'
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
            scope.popoverLikesCount = popoverLikesCount;
            scope.hiddenUsersCount = (scope.likes.length - 1) - popoverLikesCount;

            var html = $templateCache.get('messageLikeListPopoverTemplate.html');
            var popoverContent = $compile(html)(scope);

            element.popover({
                html: true,
                content: popoverContent,
                placement: scope.popoverPlacement,
                trigger: 'hover',
            }).click(function (event) {
                angular.element(this).popover('hide');
                event.stopPropagation();
            });
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
        vm.currentTab = popoverScope.modalLikesTab;
        vm.likeTypes = likeTypes;
        vm.showAllLikesTab = true;

        vm.switchTab = switchTab;
        vm.closeModal = closeModal;

        init();

        //////////

        function init() {
            if(!canShowAllLikesTab()) {
                vm.showAllLikesTab = false;
            }

            // By default show all likes
            if(vm.currentTab === undefined) {
                vm.currentTab = vm.likeTypes.length;
            }
        }

        function switchTab(tabIndex) {
            vm.currentTab = tabIndex;
        }

        function canShowAllLikesTab() {
            var receivedLikes = vm.modalLikes.filter(function(likes) {
                return likes.length === 0;
            })

            return receivedLikes.length !== vm.likeTypes.length - 1;
        }

        function closeModal() {
            $uibModalInstance.close();
        }
    }
}());
