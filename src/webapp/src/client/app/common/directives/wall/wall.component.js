(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .constant('wallImageConfig', {
            thumbHeight: 80
        })
        .component('aceWall', {
            bindings: {
                isWallModule: '=',
                wallId: '=',
                isAdmin: '=',
                hasHashtagify: '='
            },
            templateUrl: 'app/common/directives/wall/wall.html',
            controller: wallController,
            controllerAs: 'vm'
        });

    wallController.$inject = [
        '$state',
        '$scope',
        '$window',
        'wallService'
    ];

    function wallController($state, $scope, $window, wallService) {
        /*jshint validthis: true */
        var vm = this;
        $window.onscroll = scrollHandler;
        vm.wallServiceData = wallService.wallServiceData;
        vm.getCurrentWallId = wallService.getCurrentWallId;
        vm.stateParams = $state.params;

        vm.createPost = createPost;
        vm.reloadWall = reloadWall;

        init();
        ////////

        function init() {
            wallService.initWall(vm.isWallModule, vm.wallId);
            $scope.$on('$destroy', function () {
                $window.onscroll = null;
            });
        }

        function createPost(post) {
            wallService.createPost(post, vm.isWallModule);
        }

        function reloadWall() {
            wallService.reloadWall(vm.isWallModule);
        }

        function scrollHandler() {
            var documentElement = document.documentElement;
            var windowHeight = window.innerHeight ? window.innerHeight : document.body.clientHeight;
            var scrollTop = documentElement.scrollTop ? documentElement.scrollTop : document.body.scrollTop;
            var scrollPosition = window.pageYOffset ? window.pageYOffset : scrollTop;

            if (document.body.scrollHeight <= (scrollPosition + windowHeight) + 400) {
                if (wallService.wallServiceData.isScrollingEnabled) {
                    wallService.getPagedWall();
                }
            }
        }
    }
}());
