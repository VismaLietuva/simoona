(function() {
    'use strict';

    angular
        .module('simoonaApp.Banners')
        .directive('aceBannersWidget', bannersWidget);

    function bannersWidget() {
        var directive = {
            restrict: 'E',
            scope: {
                banners: '=?'
            },
            templateUrl:'app/banners/banners-widget/banners-widget.html',
            bindToController: true,
            controller: bannersWidgetController,
            controllerAs: 'vm'
        };
        return directive;
    }

    bannersWidgetController.$inject = [];

    function bannersWidgetController() {
        var vm = this;
        vm.banners = [];
    }
}());
