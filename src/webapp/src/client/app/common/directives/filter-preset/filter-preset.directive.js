(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceFilterPreset', filterPreset)
        .constant('filterPageTypes', {
            eventReportList: 0,
            eventReport: 1,
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

                    // Reload <select> in case the selected preset was changed through code
                    if (scope.selectedPresetWasDeleted)
                    {
                        reloadSelect();

                        scope.selectedPresetWasDeleted = false;
                    }
                });
            });

            scope.reloading = false;
            scope.selectedPresetWasDeleted = false;

            scope.sendSelectedPresetValues = sendSelectedPresetValues;
            scope.loadFilterPresets = loadFilterPresets;

            loadFilterPresets(scope.filterPageType);

            function reloadSelect() {
                scope.reloading = true;

                setTimeout(() => {
                    scope.reloading = false;
                    sendSelectedPresetValues(scope.selectedPreset);
                });
            }

            function loadFilterPresets(filterPageType) {
                filterPresetRepository.getPresetsForPage(filterPageType).then(
                    function (response) {
                        scope.loadedPresets = response;

                        var presetToSend = getDefaultPreset();

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
                if (scope.selectedPresetWasDeleted) {
                    return;
                }

                if (!selectedPreset) {
                    scope.onValueChange({
                        preset: undefined
                    });

                    return;
                }

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
        vm.areNamesEqual = areNamesEqual;

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
                function (result) {
                    if (result.createdPresets.length > 0) {
                        // Set preset Ids that match the backend
                        modifiedPresets.presetsToCreate = result.createdPresets;
                    }

                    syncControlsWithPresets(modifiedPresets);

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
            for (var preset of controls.presetsToCreate) {
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
                if (controls.presetsToDelete.find(x => x === preset.id)) {
                    if (scope.selectedPreset !== null && preset.id === scope.selectedPreset.id) {
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
                    scope.selectedPresetWasDeleted = true;
                    scope.selectedPreset = vm.presets[0];
                }
            }
        }

        function cancelUpdate() {
            $uibModalInstance.close();
        }

        function getModifiedPresets() {
            var presetsToDelete = [];
            var presetsToCreate = [];
            var presetsToUpdate = [];

            for (var i = 0; i < vm.controls.length; i++) {
                var preset = vm.controls[i];

                if (preset.isNew && preset.isDeleted) {
                    continue;
                }

                if (preset.isDeleted && !preset.isNew) {
                    presetsToDelete.push(preset.id);
                } else if (preset.isNew && !preset.isDeleted) {
                    presetsToCreate.push({
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
                presetsToDelete,
                presetsToCreate,
                presetsToUpdate,
            };
        }

        function isNameUnique(control) {
            if (control.name === '') {
                return true;
            }

            for (var c of vm.controls) {
                if (c.name === control.name && c.id !== control.id && !c.isDeleted) {
                    return false;
                }
            }

            return true;
        }

        function areNamesEqual() {
            var names = new Map();

            for (var control of vm.controls) {
                if (names.has(control.name)) {
                    return false;
                }

                if (control.isDeleted) {
                    continue;
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
