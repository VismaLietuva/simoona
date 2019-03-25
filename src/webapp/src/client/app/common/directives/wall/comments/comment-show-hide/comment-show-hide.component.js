(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceCommentShowHide', {
            replace: true,
            templateUrl: 'app/common/directives/wall/comments/comment-show-hide/comment-show-hide.html',
            bindings: {
                isSinglePost: '=',
                isCollapsed: '=',
                commentsCount: '='
            },
            controller: commentShowHideController,
            controllerAs: 'vm'
        });

    commentShowHideController.$inject = [];

    function commentShowHideController() {
        /*jshint validthis: true */
        var vm = this;
        
  
        vm.isCollapsed = !vm.isSinglePost;
        vm.isVisible = isVisible;
        vm.toggleCollapse = toggleCollapse;

        function isVisible() {
            return vm.commentsCount > 2;
        }

        function toggleCollapse() {
            vm.isCollapsed = !vm.isCollapsed;
        }
    }
}());