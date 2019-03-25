(function () {

    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('inputWrapper', inputWrapper);

    function inputWrapper() {

        return {
            restrict: 'EA',
            scope: {
                labelText: '@',
                inputValue: '=',
                isPrivate: '=',
                labelClasses: '@',
                classes: '@',
                customValidityMessage: '=',
                exclamationMark: '='
            },
            templateUrl: 'app/common/directives/input-wrapper/input-wrapper.html',
            transclude: true,
            controller: controller,
            link: link
        }
    }

    function link(scope, element, attrs) {
        var exclamatedTypes = ['text', 'email', 'tel', 'url'];
        var clickableInputTypes = ['checkbox', ''];
        var input = element.find('[wrapped]');

        if (input) {
            scope.inputType = input.attr('type');
            scope.inputName = input.attr('name');
            scope.inputId = input.attr('id');
            scope.charLimit = input.attr('ng-maxlength');
            scope.minlength = input.attr('ng-minlength');

            scope.isRequired = !!input.attr('required');
            scope.isExclamationMarkEnabled = scope.exclamationMark ? exclamatedTypes.indexOf(scope.inputType !== -1) : false;
            scope.canBePrivate = !!attrs.isPrivate;
        }
    }

    controller.$inject = ['$scope'];

    function controller($scope) {

        $scope.$watch('customValidityMessage', function (newVal) {
            $scope.inputValue.$setValidity('customValidity', !newVal); // No customValidityMessage - no error.
        });

        $scope.errors = {
            hasError: function () {
                return $scope.inputValue.$invalid
                    && $scope.inputValue.$dirty
                    || $scope.inputValue.$error.customValidity;
            },
            showExclamationMark: function () {
                return $scope.isExclamationMarkEnabled
                    && $scope.inputValue.$invalid
                    && $scope.inputValue.$dirty;
            },
            required: function () {
                return $scope.inputValue.$error.required
                    && $scope.inputValue.$dirty
                    || $scope.inputValue.$error.customRequired
                    && $scope.inputValue.$dirty;
            },
            charMaxLimit: function () {
                return $scope.inputValue.$error.maxlength
                    && $scope.inputValue.$dirty;
            },
            charMinLimit: function () {
                return $scope.inputValue.$error.minlength
                    && $scope.inputValue.$dirty;
            },
            email: function () {
                return $scope.inputValue.$error.email
                    && $scope.inputValue.$dirty;
            },
            url: function () {
                return $scope.inputValue.$error.url
                    && $scope.inputValue.$dirty;
            },
            date: function () {
                return $scope.inputValue.$error.date
                    && $scope.inputValue.$dirty;
            },
            minDate: function () {
                return $scope.inputValue.$error.minDate
                    && $scope.inputValue.$dirty;
            },
            maxDate: function () {
                return $scope.inputValue.$error.maxDate
                    && $scope.inputValue.$dirty;
            },
            phone: function () {
                return $scope.inputValue.$error.phone
                    && $scope.inputValue.$dirty;
            },
            compareTo: function () {
                return $scope.inputValue.$error.compareTo
                    && $scope.inputValue.$dirty;
            },
            containsDigit: function () {
                return $scope.inputValue.$error.containsDigit
                    && $scope.inputValue.$dirty;
            },
            containsLowerCase: function () {
                return $scope.inputValue.$error.containsLowerCase
                    && $scope.inputValue.$dirty;
            },
            containsUpperCase: function () {
                return $scope.inputValue.$error.containsUpperCase
                    && $scope.inputValue.$dirty;
            }
        }
    }
})();
