(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .directive('aceUpcomingEventsWidget', upcomingEventsWidget);

    function upcomingEventsWidget() {
        var directive = {
            restrict: 'E',
            scope: {
                events: '=?'
            },
            templateUrl:'app/events/upcoming-events-widget/upcoming-events-widget.html',
            bindToController: true,
            controller: upcomingEventsWidgetController,
            controllerAs: 'vm'
        };
        return directive;
    }

    upcomingEventsWidgetController.$inject = [
        'smallAvatarThumbSettings'
    ];

    function upcomingEventsWidgetController(smallAvatarThumbSettings) {
        var vm = this;
        vm.smallAvatarThumbSettings = smallAvatarThumbSettings;
    }
}());
