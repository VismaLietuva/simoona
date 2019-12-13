(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventParticipantList', {
            bindings: {
                participants: '=',
                onLeaveEvent: '&',
                isDeleteVisible: '='
            },
            templateUrl: 'app/events/content/participants/participant-list/participant-list.html',
            controller: eventParticipantListController,
            controllerAs: 'vm'
        });

    function eventParticipantListController() {
        /* jshint validthis: true */
        var vm = this;
        vm.isExpanded = false;
        
        vm.expandCollapseText = 'events.expand';
        vm.toggleExpandCollapse = toggleExpandCollapse;

        function toggleExpandCollapse() {
            if(vm.isExpanded) {
                vm.expandCollapseText = 'events.expand';
            }
            else {
                vm.expandCollapseText = 'events.collapse';
            }
            vm.isExpanded = !vm.isExpanded;
        }
    }
})();
