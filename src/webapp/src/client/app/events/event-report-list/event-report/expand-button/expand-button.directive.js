(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .directive('aceExpandButton', expandButton);

    function expandButton() {
        var directive = {
            templateUrl: 'app/events/event-report-list/event-report/expand-button/expand-button.html',
            restrict: 'E',
            replace: true,
            scope: {
                isExpanded: '=',
            },
            controller: expandButtonController,
            controllerAs: 'vm',
            bindToController: true,
        }

        return directive;
    }

    function expandButtonController() {
        var vm = this;

        vm.toggleExpandCollapse = toggleExpandCollapse;

        function toggleExpandCollapse() {
            vm.isExpanded = !vm.isExpanded;
        }
    }
})();
