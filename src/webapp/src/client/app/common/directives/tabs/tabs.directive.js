(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceTabs', tabs);

    function tabs() {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/tabs/tabs.html',
            scope: {
                tabs: '='
            },
            controller: tabsController,
            controllerAs: 'vm',
            bindToController: true
        };

        return directive;
    }

    tabsController.$inject = ['$state'];

    function tabsController($state) {
        /* jshint validthis: true */
        var vm = this;
        vm.$state = $state;
    }
})();
