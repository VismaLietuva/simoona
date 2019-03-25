(function () {

    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('autocomplete', autocomplete);

    function autocomplete() {
        return {
            restrict: 'E',
            scope: {
                shown: '@',
                boundModel: '=?',
                loadData: '=',
                withImage: '@',
                allowCreating: '@',
                isSecondary: '@',
                customValidityMessage: '=',
                formField: '=',
                parameter: '=?'
            },
            templateUrl: 'app/common/directives/autocomplete/autocomplete.html',
            controller: controller
        }
    }

    controller.$inject = ['$scope', '$element'];

    function controller($scope, $element) {
        var shownKeys = "";
        if ($scope.shown) {
            shownKeys = $scope.shown.split(' ');
        }
        $scope.suggestions = [];
        $scope.getShownText = getShownText;
        $scope.setBoundModel = setBoundModel;
        $scope.getSuggestions = getSuggestions;
        $scope.formatQuery = formatQuery;
        $scope.onKeyPress = onKeyPress;
        $scope.isSelected = isSelected;
        $scope.selectedIndex = -1;
        $scope.makeSelected = makeSelected;
        if ($scope.boundModel) {
            $scope.boundModel.queryEmpty = false;
        }

        $scope.$watch('boundModel', function (newVal) {
            if (angular.isObject(newVal)) {
                $scope.query = getShownText(newVal);
            }

            if ($scope.isSecondary && !newVal) {
                $scope.query = '';
            }
        }, true);

        function isAutocompleted() {
            var z = $scope.query.trim() === getShownText($scope.boundModel);
            return z;
        }

        if ($scope.formField) {
            $scope.$watch('customValidityMessage', function (newVal) {
                $scope.formField.$setValidity('customValidity', !newVal); // No customValidityMessage - no error.
            });
        }

        function formatQuery(ignoreQuery) {
            if ($scope.query && !$scope.query.trim()) $scope.selectedIndex = -1;

            if ($scope.selectedIndex === -1) format();

            function format() {
                $scope.suggestions = [];

                if ($scope.query) {
                    $scope.query.trim();
                }

                if ($scope.allowCreating && !isAutocompleted() && !ignoreQuery) {
                    alterBoundModel();
                    return;
                }

                if (!$scope.query) {
                    if (!$scope.isSecondary) setBoundModel();
                    $scope.query = '';
                    if ($scope.boundModel)
                        $scope.boundModel.queryEmpty = true;
                }

                if ($scope.boundModel && $scope.query) {
                    $scope.query = getShownText($scope.boundModel);
                }

                if (!$scope.boundModel && $scope.query) {
                    $scope.query = '';
                }

                function alterBoundModel() {
                    if ($scope.query || $scope.isSecondary) {
                        if ($scope.shown && $scope.shown.split(' ').length === 1) {
                            // Field must be atomic if it can be used for creating new model values
                            $scope.boundModel[$scope.shown] = $scope.query;
                        }
                    } else {
                        setBoundModel();
                    }
                }
            }
        }

        function getSuggestions(query) {
            if (query && $scope.parameter) {
                $scope.loadData({ query: query, parameter: $scope.parameter }).then(function (resource) {
                    $scope.suggestions = resource;
                    $scope.selectedIndex = -1;
                });
            }
            else if (query) {
                $scope.loadData(query).then(function (resource) {
                    $scope.suggestions = resource;
                    $scope.selectedIndex = -1;
                });
            }
            if (!query) {
                setBoundModel($scope.suggestions[$scope.selectedIndex]); //check if empty to trigger required input
            }
        }

        function setBoundModel(item) {
            if (item) {
                $scope.query = getShownText(item);
                $scope.suggestions = [];
                if (!$scope.boundModel) {

                    $scope.boundModel = item;
                    return;
                }
                for (var key in item) {
                    if ($scope.boundModel.hasOwnProperty(key)) {
                        $scope.boundModel[key] = item[key];
                    }
                }
                $scope.boundModel.isDirty = true;
            }
            else {
                for (var key in $scope.boundModel) {
                    if ($scope.boundModel.hasOwnProperty(key)) {
                        $scope.boundModel[key] = '';
                    }
                }
            }

        }

        function getShownText(item) {
            var textParts = shownKeys.map(function (a) { return item[a] });
            return textParts.join(' ');
        }

        function onKeyPress($event) {
            var keys = {
                40: goDown,
                38: goUp,
                13: selectActive
            }

            if (keys.hasOwnProperty($event.keyCode)) {
                keys[$event.keyCode]();
            }

            function goDown() {
                $scope.selectedIndex = $scope.selectedIndex < $scope.suggestions.length - 1
                    ? $scope.selectedIndex + 1
                    : 0;
            }

            function goUp() {
                $scope.selectedIndex = $scope.selectedIndex > 0
                    ? $scope.selectedIndex - 1
                    : $scope.suggestions.length - 1;
            }

            function selectActive() {
                $event.preventDefault();
                if ($scope.selectedIndex !== -1) {
                    setBoundModel($scope.suggestions[$scope.selectedIndex]);
                    $scope.selectedIndex = -1;
                    formatQuery(true);
                }
            }

        }

        function isSelected(index) {
            return index === $scope.selectedIndex;
        }

        function makeSelected(index) {
            $scope.selectedIndex = index;
        }
    }
})();
