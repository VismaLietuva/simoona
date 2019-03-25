(function () {
    'use strict';

    angular.module('simoonaApp.Common')
    .directive('timepickerMod', function () {
        return {
            restrict: 'EA',
            require: ['timepickerMod', '?^ngModel'],
            controller: 'TimepickerModController',
            replace: true,
            scope: {},
            templateUrl: 'app/common/directives/timepicker-mod/timepicker-mod.html',
            link: function (scope, element, attrs, ctrls) {
                var timepickerCtrl = ctrls[0], ngModelCtrl = ctrls[1];

                if (ngModelCtrl) {
                    timepickerCtrl.init(ngModelCtrl, element.find('input'));
                }
            }
        };
    })
    .constant('timepickerConfig', {
        hourStep: 1,
        minuteStep: 1,
        showMeridian: true,
        meridians: null,
        readonlyInput: false,
        mousewheel: true,
        showSpinners: true,
        disabled: false,
        disabledMinutes: false,
        maxHours: 12,
        minHours: 1
    })
    .controller('TimepickerModController', ['$scope', '$attrs', '$parse', '$log', '$locale', 'timepickerConfig', function ($scope, $attrs, $parse, $log, $locale, timepickerConfig) {
        var selected = new Date(),
            ngModelCtrl = { $setViewValue: angular.noop }, // nullModelCtrl
            meridians = angular.isDefined($attrs.meridians) ? $scope.$parent.$eval($attrs.meridians) : timepickerConfig.meridians || $locale.DATETIME_FORMATS.AMPMS;

        this.init = function (ngModelCtrl_, inputs) {
            ngModelCtrl = ngModelCtrl_;
            ngModelCtrl.$render = this.render;

            var hoursInputEl = inputs.eq(0),
                minutesInputEl = inputs.eq(1);

            var mousewheel = angular.isDefined($attrs.mousewheel) ? $scope.$parent.$eval($attrs.mousewheel) : timepickerConfig.mousewheel;
            if (mousewheel) {
                this.setupMousewheelEvents(hoursInputEl, minutesInputEl);
            }

            $scope.readonlyInput = angular.isDefined($attrs.readonlyInput) ? $scope.$parent.$eval($attrs.readonlyInput) : timepickerConfig.readonlyInput;
            this.setupInputEvents(hoursInputEl, minutesInputEl);
        };

        $scope.showSpinners = timepickerConfig.showSpinners;
        if ($attrs.showSpinners) {
            $scope.$parent.$watch($attrs.showSpinners, function (value) {
                $scope.showSpinners = value;
            });
        }

        $scope.disabled = timepickerConfig.disabled;
        if ($attrs.disabled) {
            $scope.$parent.$watch($attrs.disabled, function (value) {
                $scope.disabled = value;
            });
        }

        $scope.disabledMinutes = timepickerConfig.disabledMinutes;
        if ($attrs.disabledMinutes) {
            $scope.$parent.$watch($attrs.disabledMinutes, function (value) {
                $scope.disabledMinutes = value;
            });
        }

        var hourStep = timepickerConfig.hourStep;
        if ($attrs.hourStep) {
            $scope.$parent.$watch($parse($attrs.hourStep), function (value) {
                hourStep = parseInt(value, 10);
            });
        }

        var minuteStep = timepickerConfig.minuteStep;
        if ($attrs.minuteStep) {
            $scope.$parent.$watch($parse($attrs.minuteStep), function (value) {
                minuteStep = parseInt(value, 10);
            });
        }

        // 12H / 24H mode
        $scope.showMeridian = timepickerConfig.showMeridian;
        if ($attrs.showMeridian) {
            $scope.$parent.$watch($parse($attrs.showMeridian), function (value) {
                $scope.showMeridian = !!value;

                if (!$scope.showMeridian) {
                    minHours = 0;
                    maxHours = 23;
                }

                if (ngModelCtrl.$error.time) {
                    // Evaluate from template
                    var hours = getHoursFromTemplate(), minutes = getMinutesFromTemplate();
                    if (angular.isDefined(hours) && angular.isDefined(minutes)) {
                        selected.setHours(hours);
                        refresh();
                    }
                } else {
                    updateTemplate();
                }
            });
        }

        var maxHours = timepickerConfig.maxHours;
        if ($attrs.maxHours) {
            $scope.$parent.$watch($attrs.maxHours, function (value) {
                value = parseInt(value);
                if (value >= 0 && value < 24)
                    maxHours = value;
            });
        }

        var minHours = timepickerConfig.minHours;
        if ($attrs.minHours) {
            $scope.$parent.$watch($attrs.minHours, function (value) {
                value = parseInt(value);
                if (value >= 0 && value < 24 && value <= maxHours)
                    minHours = value;
            });
        }

        // Get $scope.hours in 24H mode if valid
        function getHoursFromTemplate() {
            var hours = parseInt($scope.hours, 10);
            var valid = ($scope.showMeridian) ? (hours >= minHours && hours <= maxHours) : (hours >= minHours && hours <= maxHours)
            if (!valid) {
                return undefined;
            }

            if ($scope.showMeridian) {
                if (hours === 12) {
                    hours = 0;
                }
                if ($scope.meridian === meridians[1]) {
                    hours = hours + 12;
                }
            }
            return hours;
        }

        function getMinutesFromTemplate() {
            var minutes = parseInt($scope.minutes, 10);
            return (minutes >= 0 && minutes < 60) ? minutes : undefined;
        }

        function pad(value) {
            return (angular.isDefined(value) && value.toString().length < 2) ? '0' + value : value;
        }

        // Respond on mousewheel spin
        this.setupMousewheelEvents = function (hoursInputEl, minutesInputEl) {
            var isScrollingUp = function (e) {
                if (e.originalEvent) {
                    e = e.originalEvent;
                }
                //pick correct delta variable depending on event
                var delta = (e.wheelDelta) ? e.wheelDelta : -e.deltaY;
                return (e.detail || delta > 0);
            };

            hoursInputEl.bind('mousewheel wheel', function (e) {
                $scope.$apply((isScrollingUp(e)) ? $scope.incrementHours() : $scope.decrementHours());
                e.preventDefault();
            });

            minutesInputEl.bind('mousewheel wheel', function (e) {
                $scope.$apply((isScrollingUp(e)) ? $scope.incrementMinutes() : $scope.decrementMinutes());
                e.preventDefault();
            });

        };

        this.setupInputEvents = function (hoursInputEl, minutesInputEl) {
            if ($scope.readonlyInput) {
                $scope.updateHours = angular.noop;
                $scope.updateMinutes = angular.noop;
                return;
            }

            var invalidate = function (invalidHours, invalidMinutes) {
                ngModelCtrl.$setViewValue(null);
                ngModelCtrl.$setValidity('time', false);
                if (angular.isDefined(invalidHours)) {
                    $scope.invalidHours = invalidHours;
                }
                if (angular.isDefined(invalidMinutes)) {
                    $scope.invalidMinutes = invalidMinutes;
                }
            };

            $scope.updateHours = function () {
                var hours = getHoursFromTemplate();

                if (angular.isDefined(hours)) {
                    selected.setHours(hours);
                    refresh('h');
                } else {
                    invalidate(true);
                }
            };

            hoursInputEl.bind('blur', function (e) {
                if (!$scope.invalidHours && $scope.hours < 10) {
                    $scope.$apply(function () {
                        $scope.hours = pad($scope.hours);
                    });
                }
            });

            $scope.updateMinutes = function () {
                var minutes = getMinutesFromTemplate();

                if (angular.isDefined(minutes)) {
                    selected.setMinutes(minutes);
                    refresh('m');
                } else {
                    invalidate(undefined, true);
                }
            };

            minutesInputEl.bind('blur', function (e) {
                if (!$scope.invalidMinutes && $scope.minutes < 10) {
                    $scope.$apply(function () {
                        $scope.minutes = pad($scope.minutes);
                    });
                }
            });

        };

        this.render = function () {
            var date = ngModelCtrl.$modelValue ? new Date(ngModelCtrl.$modelValue) : null;

            if (isNaN(date)) {
                ngModelCtrl.$setValidity('time', false);
                $log.error('Timepicker directive: "ng-model" value must be a Date object, a number of milliseconds since 01.01.1970 or a string representing an RFC2822 or ISO 8601 date.');
            } else {
                if (date) {
                    selected = date;
                }
                makeValid();
                updateTemplate();
            }
        };

        // Call internally when we know that model is valid.
        function refresh(keyboardChange) {
            makeValid();
            ngModelCtrl.$setViewValue(new Date(selected));
            updateTemplate(keyboardChange);
        }

        function makeValid() {
            ngModelCtrl.$setValidity('time', true);
            $scope.invalidHours = false;
            $scope.invalidMinutes = false;
        }

        function updateTemplate(keyboardChange) {
            var hours = selected.getHours(), minutes = selected.getMinutes();

            if ($scope.showMeridian) {
                hours = (hours === 0 || hours === 12) ? 12 : hours % 12; // Convert 24 to 12 hour system
            }

            $scope.hours = keyboardChange === 'h' ? hours : pad(hours);
            $scope.minutes = keyboardChange === 'm' ? minutes : pad(minutes);
            $scope.meridian = selected.getHours() < 12 ? meridians[0] : meridians[1];
        }

        function addMinutes(minutes) {
            var dt = new Date(selected.getTime() + minutes * 60000);

            // Handle max and min hours
            if (dt.getHours() > maxHours)
                selected.setHours(minHours, dt.getMinutes());
            else if (dt.getHours() < minHours)
                selected.setHours(maxHours, dt.getMinutes());
            else
                selected.setHours(dt.getHours(), dt.getMinutes());

            refresh();
        }

        $scope.incrementHours = function () {
            addMinutes(hourStep * 60);
        };
        $scope.decrementHours = function () {
            addMinutes(-hourStep * 60);
        };
        $scope.incrementMinutes = function () {
            addMinutes(minuteStep);
        };
        $scope.decrementMinutes = function () {
            addMinutes(-minuteStep);
        };
        $scope.toggleMeridian = function () {
            addMinutes(12 * 60 * ((selected.getHours() < 12) ? 1 : -1));
        };
    }]);
})();