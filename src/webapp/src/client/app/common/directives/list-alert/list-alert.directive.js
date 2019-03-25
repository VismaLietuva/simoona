(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceListAlert', listAlert);

    listAlert.$inject = [
        '$state',
        '$compile',
        'localeSrv'
    ];

    function listAlert($state, $compile, localeSrv) {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/common/directives/list-alert/list-alert.html',
            scope: {
                alertMsg: '@',
                linkMsg: '@',
                alertType: '@',
                state: '@'
            },
            link: linkFunc
        };

        return directive;

        function linkFunc(scope, elem) {
            /*jshint validthis: true */
            scope.onClick = onClick;

            var alertIconTypes = {
                success: 'success',
                error: 'error',
                warning: 'warning',
                info: 'info',
                help: 'help'
            };
            var alertIcon = '<span class="vismaicon vismaicon-filled vismaicon-{0}"></span>';

            init();

            ///////////

            function init() {
                if (scope.alertType) {
                    alertIcon = String.format(alertIcon, alertIconTypes[scope.alertType]);
                } else {
                    alertIcon = String.format(alertIcon, alertIconTypes.info);
                }

                if (scope.state) {
                    var html = '<a ng-click="onClick()">' + scope.linkMsg + '</a>';
                    scope.alertMsg = alertIcon + localeSrv.formatTranslation(scope.alertMsg, {one: html});
                } else {
                    scope.alertMsg = alertIcon + localeSrv.translate(scope.alertMsg);
                }

                elem.html(scope.alertMsg);
                $compile(elem.contents())(scope);
            }

            function onClick() {
                $state.go(scope.state);
            }
        }
    }
})();
