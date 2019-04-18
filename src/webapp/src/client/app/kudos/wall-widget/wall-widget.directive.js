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
        'kudosWallWidgetSettings'
    ];

    function kudosWallWidget(authService, kudosFactory, kudosWallWidgetSettings) {
        var directive = {
            restrict: 'E',
            templateUrl: 'app/kudos/wall-widget/wall-widget.html',
            link: linkFunc,
            scope: {
                lastApprovedKudos: '=?'
            }
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