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
        'wallService',
        'employeeListRepository',
    ];

    function wallController($state, $scope, $window, wallService, employeeListRepository) {
        /*jshint validthis: true */
        var vm = this;
        $window.onscroll = scrollHandler;
        vm.wallServiceData = wallService.wallServiceData;
        vm.getCurrentWallId = wallService.getCurrentWallId;
        vm.stateParams = $state.params;

        vm.createPost = createPost;
        vm.reloadWall = reloadWall;
        vm.getEmployeeList = getEmployeeList;

        init();
        ////////

        function init() {
            wallService.initWall(vm.isWallModule, vm.wallId);
            vm.getEmployeeList();
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
        
        function getEmployeeList () {
            employeeListRepository.getPaged({
                page: 1,
                search: ''
            }).then(function (getPagedResponse) {
                vm.employees = getPagedResponse.pagedList.map(cur => {
                    return {
                        label: `${cur.firstName} ${cur.lastName}`
                    };
                });
            });
        };
    }
}());
