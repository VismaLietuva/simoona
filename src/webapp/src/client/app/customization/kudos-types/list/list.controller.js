(function() {

    angular.module('simoonaApp.Customization.KudosTypes')
        .controller('kudosTypesListController', kudosTypesListController);

    kudosTypesListController.$inject = ['$rootScope', 'kudosTypesRepository', 'errorHandler'];

    function kudosTypesListController($rootScope, kudosTypesRepository, errorHandler) {
        var vm = this;
        
        $rootScope.pageTitle = 'customization.kudosTypes';
        vm.kudosTypes = [];

        init();
        
        ///////////
        
        function init() {
            kudosTypesRepository.getKudosTypes()
                .then(function(types) {
                    vm.kudosTypes = types;
                }, errorHandler.handleErrorMessage);
        }
    }
})();