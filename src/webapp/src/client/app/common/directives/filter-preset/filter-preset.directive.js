(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceFilterPreset', filterPreset)
        .constant('filterPageTypes', {
            extensiveEventDetailsList: 0,
            extensiveEventDetails: 1,
        })
        .constant('filterTypes', {
            events: 0,
            kudos: 1,
            offices: 2,
        })
        .constant('filterTypesRowTranslations',[
            'common.presetConfigurationTableEvents',
            'common.presetConfigurationTableKudos',
            'common.presetConfigurationTableName',
        ]);

    filterPreset.$inject = [
        '$uibModal',
        'filterPresetRepository',
        'notifySrv',
    ];

    function filterPreset(
        $uibModal,
        filterPresetRepository,
        notifySrv
    ) {
        var directive = {
            templateUrl:
                'app/common/directives/filter-preset/filter-preset.html',
            restrict: 'E',
            replace: true,
            scope: {
                filterPageType: '@',
                modalSize: '@',
                loadedFilterTypes: '=',
                onValueChange: '&',
            },
            link: linkFunc,
        };

        return directive;

        function linkFunc(scope, element) {
            angular.element('#link').bind('click', function () {
                // Disable scrolling while modal is open
                $(":root").css("overflow-y", 'hidden');

                $uibModal.open({
                    templateUrl:
                        'app/common/directives/filter-preset/filter-preset-modal.html',
                    windowTopClass: '',
                    controller: filterPresetController,
                    controllerAs: 'vm',
                    windowTopClass: 'preset-configuration-modal',
                    resolve: {
                        scope: scope,
                    },
                    size: scope.modalSize,
                })
                .closed
                .then(function() {
                    // Enable scrolling after modal is closed
                    $(":root").css("overflow-y", '');
                });
            });

            scope.sendSelectedPresetValues = sendSelectedPresetValues;
            scope.loadFilterPresets = loadFilterPresets;

            loadFilterPresets(scope.filterPageType);

            function loadFilterPresets(filterPageType) {
                filterPresetRepository.getPresetsForPage(filterPageType).then(
                    function (response) {
                        scope.loadedPresets = response;

                        var presetToSend = getDefaultPreset();

                        if (!presetToSend) {
                            return;
                        }

                        sendSelectedPresetValues(presetToSend);
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
            }

            function getDefaultPreset() {
                var defaultPreset = scope.loadedPresets.find(
                    (preset) => preset.isDefault
                );

                if (!defaultPreset && scope.loadedPresets.length > 0) {
                    return scope.loadedPresets[0];
                }

                return defaultPreset;
            }

            function sendSelectedPresetValues(selectedPreset) {
                scope.selectedPreset = selectedPreset;
                // Send to consumer
                scope.onValueChange({
                    preset: scope.selectedPreset,
                });
            }
        }
    }

    filterPresetController.$inject = [
        'scope',
        'filterPresetRepository',
        'filterPageTypes',
        '$uibModalInstance',
        'notifySrv',
        'filterTypesRowTranslations'
    ];

    function filterPresetController(
        scope,
        filterPresetRepository,
        filterPageTypes,
        $uibModalInstance,
        notifySrv,
        filterTypesRowTranslations
    ) {
        var vm = this;

        vm.presets = scope.loadedPresets;
        vm.filterPageType = scope.filterPageType;
        vm.filterTypesRowTranslations = filterTypesRowTranslations;

        vm.controls = null;
        vm.previousControlsState = null;

        vm.loadedFilterTypes = Object.values(scope.loadedFilterTypes);

        vm.filterPageTypes = filterPageTypes;
        vm.allCheckedAsDeleted = false;

        vm.sendSelectedPresetValues = scope.sendSelectedPresetValues;

        vm.getFilterTypes = getFilterTypes;
        vm.getFilterMapByType = getFilterMapByType;
        vm.cancelUpdate = cancelUpdate;
        vm.applyUpdate = applyUpdate;
        vm.uncheckAllSelectedDefaultPresets = uncheckAllSelectedDefaultPresets;
        vm.setPresetAsDeleted = setPresetAsDeleted;
        vm.spawnNewPresetControl = spawnNewPresetControl;
        vm.isNameUnique = isNameUnique;
        vm.isNamesUnique = isNamesUnique;

        init();


        function init() {
            vm.presets.map((preset) => (preset.isDeleted = false));

            vm.controls = [];

            for (var i = 0; i < vm.presets.length; i++) {
                var row = [];

                for (var j = 0; j < vm.loadedFilterTypes.length; j++) {
                    row.push({
                        filterType: vm.loadedFilterTypes[j].filterType,
                        appliedTypes: getFilterMapByType(
                            vm.presets[i].filters,
                            vm.loadedFilterTypes[j].filterType
                        ),
                    });
                }

                vm.controls.push({
                    id: vm.presets[i].id,
                    filters: row,
                    name: vm.presets[i].name,
                    isDefault: vm.presets[i].isDefault,
                });
            }

            vm.previousControlsState = createControlsCopy(vm.controls);
        }

        function allCheckedAsDeleted() {
            for (var control of vm.controls) {
                if (!control.isDeleted) {
                    return false;
                }
            }

            return true;
        }

        function createControlsCopy(controls) {
            return controls.map((control) => ({
                id: control.id,
                isDeleted: control.isDeleted,
                name: control.name,
                filters: control.filters.map((r) => ({
                    appliedTypes: new Map(r.appliedTypes),
                })),
                isDefault: control.isDefault,
            }));
        }

        function getFilterTypes(type) {
            return vm.loadedFilterTypes.find(
                (filter) => filter.filterType === parseInt(type)
            ).filters;
        }

        function getFilterMapByType(filters, filterType) {
            var types = getFilterTypesByType(filters, filterType);

            if (!types) {
                return new Map();
            }

            return new Map(
                types.map((type) => [
                    parseInt(type),
                    getTypeName(filterType, type),
                ])
            );
        }

        function getTypeName(filterType, type) {
            var filters = vm.loadedFilterTypes.find(
                (filter) => filter.filterType === filterType
            ).filters;

            return filters.find((filterType) => filterType.id == type).name;
        }

        function getFilterTypesByType(filters, filterType) {
            var filter = Object.values(filters).find(
                (filter) => filter.filterType === filterType
            );

            if (!filter) {
                return null;
            }

            return filter.types;
        }

        function applyUpdate() {
            var modifiedPresets = getModifiedPresets();

            filterPresetRepository.updatePresets(modifiedPresets, vm.filterPageType).then(
                function () {
                    syncControlsWithPresets(modifiedPresets);

                    // If new presets are created, we need to fetch data from the backend again so we can get the new presets' created ids.
                    // Otherwise, updating the newly created preset will fail.
                    if (modifiedPresets.presetsToAdd.length) {
                        scope.loadFilterPresets(scope.filterPageType);
                    }

                    notifySrv.success('common.infoSaved');
                    $uibModalInstance.close();
                },
                function () {
                    notifySrv.error('errorCodeMessages.messageError');
                }
            );
        }

        function syncControlsWithPresets(controls) {
            // Add newly created presets
            for (var preset of controls.presetsToAdd) {
                vm.presets.push(preset);
            }

            // Update existing presets in vm.presets
            for (var preset of controls.presetsToUpdate) {
                var cur = vm.presets.find(p => p.id === preset.id);

                if (!cur) {
                    continue;
                }

                cur.isDeleted = preset.isDeleted;
                cur.name = preset.name;
                cur.isDefault = preset.isDefault;
                cur.filters = preset.filters;
            }

            // Removing from loading presets presets that were checked as removed by the user
            scope.loadedPresets = vm.presets.filter(function (preset) {
                if (controls.presetsToRemove.find(x => x === preset.id)) {
                    if (preset.id === scope.selectedPreset.id) {
                        scope.selectedPreset = null;
                    }

                    return false;
                }

                return true;
            });

            vm.presets = scope.loadedPresets;

            // Selecting the first preset if in previous steps the default/selected preset was removed
            if (!scope.selectedPreset) {
                if (vm.presets.length > 0) {
                    scope.selectedPreset = vm.presets[0];
                }
            }
        }

        function cancelUpdate() {
            $uibModalInstance.close();
        }

        function getModifiedPresets() {
            var presetsToRemove = [];
            var presetsToAdd = [];
            var presetsToUpdate = [];

            for (var i = 0; i < vm.controls.length; i++) {
                var preset = vm.controls[i];

                if (preset.isDeleted && !preset.isNew) {
                    presetsToRemove.push(preset.id);
                } else if (preset.isNew && !preset.isDeleted) {
                    presetsToAdd.push({
                        ...preset,
                        filters: mapControlFiltersToPresetFilters(preset)
                    });
                } else if (isPresetUpdated(i)) {
                    presetsToUpdate.push({
                        ...preset,
                        filters: mapControlFiltersToPresetFilters(preset),
                    });
                }
            }

            return {
                presetsToRemove,
                presetsToAdd,
                presetsToUpdate,
            };
        }

        function isNameUnique(control) {
            if (control.name === '') {
                return true;
            }

            for (var c of vm.controls) {
                if (c.name === control.name && c.id !== control.id) {
                    return false;
                }
            }

            return true;
        }

        function isNamesUnique() {
            var names = new Map();

            for (var control of vm.controls) {
                if (names.has(control.name)) {
                    return false;
                }

                names.set(control.name, true);
            }

            return true;
        }

        function mapControlFiltersToPresetFilters(controlPreset) {
            return controlPreset.filters.map((filter) => ({
                filterType: filter.filterType,
                types: [...filter.appliedTypes.keys()],
            }));
        }

        function setPresetAsDeleted(preset) {
            preset.isDeleted = true;
            vm.allCheckedAsDeleted = allCheckedAsDeleted();
        }

        function uncheckAllSelectedDefaultPresets(preset) {
            for (var control of vm.controls) {
                if (control.id != preset.id) {
                    control.isDefault = false;
                }
            }
        }

        function spawnNewPresetControl() {
            var filters = [];

            for (var j = 0; j < vm.loadedFilterTypes.length; j++) {
                filters.push({
                    filterType: vm.loadedFilterTypes[j].filterType,
                    appliedTypes: new Map(),
                });
            }

            vm.controls.push({
                // To ensure that uncheckAllSelectedDefaultPresets() works properly,
                // we need to uniquely identify new presets that have not yet been created in the backend.
                id: String.fromCharCode(vm.controls.length + 1),
                filters: filters,
                isDefault: false,
                isNew: true,
            });

            vm.allCheckedAsDeleted = false;
        }

        function isPresetUpdated(index) {
            var previousControlState = vm.previousControlsState[index];
            var control = vm.controls[index];

            if (control.name !== previousControlState.name) {
                return true;
            }

            if (control.isDefault !== previousControlState.isDefault) {
                return true;
            }

            for (var i = 0; i < control.filters.length; i++) {
                if (
                    areAppliedTypesUpdated(
                        control.filters[i].appliedTypes,
                        previousControlState.filters[i].appliedTypes
                    )
                ) {
                    return true;
                }
            }

            return false;
        }

        function areAppliedTypesUpdated(previousTypes, types) {
            if (previousTypes.size !== types.size) {
                return true;
            }

            for (var key of types.keys()) {
                if (!previousTypes.has(key)) {
                    return true;
                }
            }

            return false;
        }
    }
})();
