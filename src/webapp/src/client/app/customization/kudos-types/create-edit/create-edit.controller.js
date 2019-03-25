(function() {

    angular.module('simoonaApp.Customization.KudosTypes')
        .controller('createEditKudosTypesController', createEditKudosTypesController);

    createEditKudosTypesController.$inject = ['$rootScope', '$stateParams', '$state', 'kudosTypesRepository', 'errorHandler'];

    function createEditKudosTypesController($rootScope, $stateParams, $state, kudosTypesRepository, errorHandler) {
        var vm = this;
        var listState = 'Root.WithOrg.Admin.Customization.KudosTypes.List';

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Admin.Customization.KudosTypes.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Customization.KudosTypes.Edit')
        };
        
        vm.isLoading = vm.states.isCreate ? false : true;

        if(vm.states.isCreate) {
            $rootScope.pageTitle = 'customization.createKudosType';
        } else {
            $rootScope.pageTitle = 'customization.editKudosTypes';
        }

        vm.kudosType = {};

        vm.createKudosType = createKudosType;
        vm.updateKudosType = updateKudosType;
        vm.disableKudosType = disableKudosType;

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
                        vm.isLoading = false;
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
                description: vm.kudosType.description
                })
                .then(function() {
                    $state.go(listState);
                }, errorHandler.handleErrorMessage);
        }

        function disableKudosType() {
            kudosTypesRepository.disableType(vm.kudosType.id)
                .then(function() {
                    $state.go(listState);
                }, errorHandler.handleErrorMessage);
        }
    }
})();