(function () {
    'use strict';

    angular.module('simoonaApp.Common')
        .directive('saveBackInfoError', saveBackInfoError);

    saveBackInfoError.$inject = [];

    function saveBackInfoError() {

        return {
            templateUrl: 'app/common/directives/save-back-info/saveBackInfo.html',
            restrict: 'E',
            scope: {
                errors: '=',
                infos: '=',
                saveDisabled: '=',
                onSave: '=',
                onBack: '=',
                showRequired: '='
            },
            controller: ['$scope', '$timeout', function ($scope, $timeout) {

                function changedErrorsValue(newVal, oldVal) {
                    $scope.errorsList = null;
                    $scope.errorItem = "";

                    if (newVal === Array)
                        $scope.errorsList = newVal;
                    else if (newVal !== undefined)
                        $scope.errorItem = newVal;
                }

                function onSaveButtonClick() {
                    $scope.errors = [];
                    $scope.infos = [];
                    if ($scope.onSave !== undefined && $scope.onSave !== null)
                        $scope.onSave();
                }

                function onBackButtonClick() {
                    if ($scope.onBack !== undefined && $scope.onBack !== null)
                        $scope.onBack();
                    else
                        goBack();
                }

                function init() {
                    changedErrorsValue($scope.errors, "");
                }

                function isBackAvailable() {
                    if (document.referrer == "") {
                        return false;
                    } else {
                        return true;
                    }
                }

                $scope.isBackAvailable = isBackAvailable;
                $scope.saveButtonClick = onSaveButtonClick;
                $scope.backButtonClick = onBackButtonClick;
                $scope.$watch('errors', changedErrorsValue);

                init();
            }]
        }
    }
})();
