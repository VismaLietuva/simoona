(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .component('aceWallHeader', {
            bindings: {
                header: '='
            },
            templateUrl: 'app/wall/item/header/header.html',
            controller: wallHeaderController,
            controllerAs: 'vm'
        });

    wallHeaderController.$inject = [
        'wallRepository',
        'wallService',
        'errorHandler'
    ];

    function wallHeaderController(wallRepository, wallService, errorHandler) {
        /*jshint validthis: true */
        var vm = this;

        vm.tabs = [];
        vm.wallServiceData = wallService.wallServiceData;

        init();

        ////////

        function init() {
            addTabs();
        }

        function addTabs() {
            vm.tabs = [
                {
                    resource: 'wall.feed',
                    state: 'Root.WithOrg.Client.Wall.Item.Feed',
                    params: {
                        wall: vm.header.id
                    },
                    testId: 'wall-feed'
                },
                {
                    resource: 'wall.members',
                    state: 'Root.WithOrg.Client.Wall.Item.Members',
                    params: {
                        wall: vm.header.id
                    },
                    badgeData: {
                        data: vm.wallServiceData.wallHeader,
                        key: 'totalMembers'
                    },
                    testId: 'wall-members'
                }
            ];
        }
    }
}());
