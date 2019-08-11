(function () {
    'use strict';

    angular
        .module('simoonaApp.UserBar')
        .directive('aceInfiniteScroll', [
            '$rootScope', '$window', '$timeout', function($rootScope, $window, $timeout) {
                return {
                    restrict: 'A',
                    scope: {
                        scrollDisabled: '=',
                        scrollDistance: '=',
                        immediateCheck: '=',
                        onget: '=',
                        itemsToAdd: '@',
                        defaultElementWidth: '@'
                    },
                    link: function(scope, elem, attrs) {
                        var checkWhenEnabled, handler, scrollEnabled;
                        $window = angular.element($window);
                        var scrollDistance = 0;
                        var container = elem.parent();

                        if (!scope.itemsToAdd) {
                            scope.itemsToAdd = 5;
                        }

                        if (!scope.defaultElementWidth) {
                            scope.defaultElementWidth = 130;
                        }

                        if (scope.scrollDistance != null) {
                            scope.$watch(scope.scrollDistance, function(value) {
                                return scrollDistance = parseInt(value, 10);
                            });
                        }

                        scrollEnabled = true;
                        checkWhenEnabled = false;

                        if (scope.scrollDisabled != null) {
                            scope.$watch(scope.scrollDisabled, function(value) {
                                scrollEnabled = !value;

                                if (scrollEnabled && checkWhenEnabled) {
                                    checkWhenEnabled = false;
                                    return handler();
                                }
                            });
                        }

                        handler = function() {
                            $timeout(function() {
                                var elementEdge, remaining, shouldScroll, containerEdge, scrollAmount;

                                elementEdge = elem.prop('scrollWidth');
                                containerEdge = container.width() + container.scrollLeft();
                                scrollAmount = container.width() * scrollDistance;
                                remaining = elementEdge - containerEdge;
                                shouldScroll = remaining <= scrollAmount || elem.children().length === 0;

                                if (shouldScroll && scrollEnabled) {
                                    var lastElement = elem.children().last();
                                    var elementWidth;
                                    var lastElementEdge;

                                    if (lastElement.length === 0 || lastElement.width() === 0) {
                                        elementWidth = scope.defaultElementWidth;
                                        lastElementEdge = 0;
                                    } else {
                                        elementWidth = lastElement.width();
                                        lastElementEdge = lastElement.position().left;
                                    }

                                    lastElementEdge = lastElementEdge + elementWidth;

                                    // Calculate items count to fill remaining space
                                    scope.onget((container.width() - lastElementEdge) / elementWidth, parseInt(scope.itemsToAdd));
                                } else if (shouldScroll) {
                                    checkWhenEnabled = true;
                                }
                            }, 0);
                        };

                        container.on('scroll', function() {
                            handler();
                        });

                        scope.$on('$destroy', function() {
                            return container.off('scroll', handler);
                        });

                        scope.$on('executeInfiniteScrollLoad', function() {
                            if (container.scrollLeft() > 0) {
                                // If container is already scrolled, scroll it to 0 - then onScroll will fire and execute handler()
                                container.scrollLeft(0);
                            } else {
                                // Execute handler() manually, because container is not scrolled and onScroll will not fire handler()
                                handler();
                            }
                        });

                        return $timeout(function() {
                            if (scope.immediateCheck) {
                                return handler();
                            }
                        }, 0);
                    }
                };
            }
        ]
    );
})();
