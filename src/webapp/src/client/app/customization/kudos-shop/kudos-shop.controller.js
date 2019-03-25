(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.KudosShop')
        .controller('kudosShopController', kudosShopController);

    kudosShopController.$inject = [
        '$rootScope',
        'kudosShopRepository',
        'errorHandler'
    ];

    function kudosShopController($rootScope, kudosShopRepository, errorHandler) {
        var vm = this;

        $rootScope.pageTitle = 'customization.kudosShop';
        
        vm.isLoading = true;
        vm.kudosShopItems = [];

        init();

        ///////////

        function init() {
            kudosShopRepository.getKudosShopItems().then(function(response) {
                vm.kudosShopItems = response;
                vm.isLoading = false;
            }, errorHandler.handleErrorMessage);
        }
    }
})();