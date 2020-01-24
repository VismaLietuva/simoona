(function() {

    angular.module('simoonaApp.Customization.KudosTypes')
        .controller('createEditKudosTypesController', createEditKudosTypesController);

    createEditKudosTypesController.$inject = ['$rootScope', '$stateParams', '$state', 'kudosTypesRepository', 'errorHandler', 'definedKudosTypes'];

    function createEditKudosTypesController($rootScope, $stateParams, $state, kudosTypesRepository, errorHandler, definedKudosTypes) {
        var vm = this;
        var listState = 'Root.WithOrg.Admin.Customization.KudosTypes.List';

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Admin.Customization.KudosTypes.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Customization.KudosTypes.Edit')
        };

        vm.isLoading = vm.states.isCreate ? false : true;

        if(vm.states.isCreate) {
            $rootScope.pageTitle = 'customization.createKudosType';
            vm.isEditable = true;
        } else {
            $rootScope.pageTitle = 'customization.editKudosTypes';
        }

        vm.kudosType = {};

        vm.toggleVisibility = toggleVisibility;

        vm.createKudosType = createKudosType;
        vm.updateKudosType = updateKudosType;
        vm.removeKudosType = removeKudosType;

        init();
        //////////

        function init() {
            if($stateParams.id) {
                kudosTypesRepository.getKudosType($stateParams.id)
                    .then(function(type) {
                        vm.kudosType.id = type.id;
                        vm.kudosType.name = type.name; 
                        vm.kudosType.multiplier = parseInt(type.value);
                        vm.kudosType.description = type.description;
                        vm.kudosType.isActive = type.isActive;
                        vm.isLoading = false;
                        if(vm.states.isEdit) {vm.isEditable = type.type === definedKudosTypes.ordinary ? true : false;}
                        vm.isVisibilityToggleable = type.type !== definedKudosTypes.send ? true : false;
                    }, function (error) {
                    errorHandler.handleErrorMessage(error);
                    $state.go(listState);
                });
            }
        }

        function createKudosType() {
            kudosTypesRepository.createKudosType(vm.kudosType).then(function() {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
            
        }

        function updateKudosType() {
            kudosTypesRepository.updateKudosType({
                id: vm.kudosType.id, 
                name: vm.kudosType.name, 
                value: vm.kudosType.multiplier, 
                description: vm.kudosType.description,
                isActive: vm.kudosType.isActive
                })
                .then(function() {
                    $state.go(listState);
                }, errorHandler.handleErrorMessage);
        }

        function removeKudosType() {
            kudosTypesRepository.removeType(vm.kudosType.id)
                .then(function() {
                    $state.go(listState);
                }, errorHandler.handleErrorMessage);
        }

        function toggleVisibility() {
            vm.kudosType.isActive = !vm.kudosType.isActive;
        }
    }
})();