(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .constant('kudosWallWidgetSettings', {
            approvedKudosCount: 5
        })
        .directive('aceKudosWallWidget', kudosWallWidget);

    kudosWallWidget.$inject = [
        'authService',
        'kudosFactory',
        'modalTypes',
        'kudosWallWidgetSettings'
    ];

    function kudosWallWidget(authService, kudosFactory, modalTypes, kudosWallWidgetSettings) {
        var directive = {
            restrict: 'E',
            templateUrl: 'app/kudos/wall-widget/wall-widget.html',
            link: linkFunc,
            scope: {
                lastApprovedKudos: '=?'
            },
            controller: function(){
                var vm = this;
                vm.modalTypes = modalTypes;
            },
            controllerAs: 'vm'
        };
        return directive;

        function linkFunc(scope) {
            scope.kudosWallWidgetSettings = kudosWallWidgetSettings;
            scope.lastApprovedKudos = [];
            scope.hasKudosPermissions = authService.hasPermissions(['KUDOS_BASIC']);

            scope.$on('addKudosEvent', function (event, args) {
                getLastApprovedKudos();
            });
            
            ////////

            function getLastApprovedKudos() {
                kudosFactory.getLastApprovedKudos(kudosWallWidgetSettings.approvedKudosCount).then(function (result) {
                    scope.lastApprovedKudos = result;
                });
            }
        }
    }
})();