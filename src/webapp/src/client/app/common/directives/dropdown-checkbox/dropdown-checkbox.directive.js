(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceDropdownCheckbox', dropdownCheckbox);

    function dropdownCheckbox() {
        var directive = {
            templateUrl: 'app/common/directives/dropdown-checkbox/dropdown-checkbox.html',
            restrict: 'E',
            replace: true,
            scope: {
                types: '=',
                onValueChange: '&',
                translation: '@'
            },
            controller: dropdownCheckboxController,
            controllerAs: 'vm',
            bindToController: true,
        }

        return directive;
    }

    function dropdownCheckboxController() {
        var vm = this;

        vm.appliedTypes = new Map();

        vm.toggleType = toggleType;
        vm.isTypeApplied = isTypeApplied;


        function isTypeApplied(type) {
            return vm.appliedTypes.has(type.id);
        }

        function toggleType(type) {
            if(vm.appliedTypes.has(type.id)) {
                vm.appliedTypes.delete(type.id);
            } else {
                vm.appliedTypes.set(type.id, true);
            }

            // Send to consumer current selected types
            vm.onValueChange({
                types: [...vm.appliedTypes.keys()]
            });
        }
    }
})();
