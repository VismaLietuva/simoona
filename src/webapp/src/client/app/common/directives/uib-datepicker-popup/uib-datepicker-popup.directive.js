(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('uibDatepickerPopup', datepickerPopup);

    datepickerPopup.$inject = ['$locale', 'dateFilter', 'uibDateParser', 'uibDatepickerPopupConfig'];

    function datepickerPopup($locale, dateFilter, uibDateParser, uibDatepickerPopupConfig) {
        var directive = {
            restrict: 'A',
            priority: 1,
            require: 'ngModel',
            link: function (scope, element, attr, ngModel) {
                var dateFormat = attr.uibDatepickerPopup || uibDatepickerPopupConfig.datepickerPopup;

                ngModel.$validators.date = function (modelValue, viewValue) {
                    var value = viewValue || modelValue;

                    if (!attr.ngRequired && !value) {
                        return true;
                    }

                    if (angular.isNumber(value)) {
                        value = new Date(value);
                    }

                    if (!value) {
                        return true;
                    } else if (angular.isDate(value) && !isNaN(value)) {
                        return true;
                    } else if (angular.isString(value)) {
                        var date = uibDateParser.parse(value, dateFormat);
                        return !isNaN(date);
                    } else {
                        return false;
                    }
                };
            }
        };
        return directive;
    }

})();
