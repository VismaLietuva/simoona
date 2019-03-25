(function () {
    'use strict';

    angular
        .module('simoonaApp.UserBar')
        .directive('usersBar', [

        '$rootScope', function($rootScope) {
            return {
                controller: 'usersBarController',
                restrict: 'AE',
                replace: true,
                transclude: true,
                templateUrl: 'app/office/map/user-bar/user-bar.html',
                scope: {
                    params: '='
                },
                link: function(scope, element, attrs) {
                    var previousPosition = { left: 0, top: 0 };

                    scope.$watch('params.floorId',
                        function(newValue, oldValue) {
                            if (!scope.params.user && newValue !== oldValue) {
                                if (scope.params.floorId) {
                                    scope.loadData();
                                }
                            }
                        });

                    scope.$watch('params.roomId',
                        function(newValue, oldValue) {
                            if (newValue !== oldValue) {
                                if (scope.params.roomId) {
                                    scope.loadData();
                                }
                                else {
                                    scope.revertToAllUsersMode();
                                }
                            }
                        });

                    angular.element('.users-bar .container').scroll(function(e) {
                        var element = angular.element(e.target);

                        var popover = angular.element('.popover');

                        if (popover.length > 0) {
                            popover.css('left', popover.position().left - element.scrollLeft() + previousPosition.left);
                        }

                        previousPosition.left = element.scrollLeft();
                    });

                    angular.element(window).resize(function(e) {
                        var windowEl = angular.element(e.target);
                        var usersBar = angular.element('.users-bar .container');
                        var popover = angular.element('.popover');

                        if (popover.length > 0) {
                            popover.css('top', usersBar.offset().top - popover.height());
                        }
                    }).scroll(function(e) {
                        var windowEl = angular.element(e.target);
                        var popover = angular.element('.popover');

                        if (popover.length > 0) {
                            popover.css('top', popover.position().top + windowEl.scrollTop() - previousPosition.top);
                        }

                        previousPosition.top = windowEl.scrollTop();
                    });
                }
            };
        }
    ]);
})();