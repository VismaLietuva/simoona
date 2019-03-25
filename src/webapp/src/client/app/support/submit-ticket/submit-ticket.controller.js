(function () {
    'use strict'; 

    angular.module('simoonaApp.Support')
        .constant('supportSettings', {
            subjectLength: 300,
            messageLength: 5000
        })
        .constant('supportTypesResources', {
            0: 'Bug',
            1: 'Incident',
            2: 'FeatureRequest',
            3: 'Question'
        })
        .controller('supportController', supportController); 

    supportController.$inject = [
        '$state', 
        '$rootScope',    
        'supportRepository', 
        'notifySrv', 
        'errorHandler',
        'supportSettings',
        'supportTypesResources'
    ]; 

    function supportController($state, $rootScope, supportRepository, notifySrv, errorHandler, supportSettings, supportTypesResources) {
        
        $rootScope.pageTitle = 'support.submitTicket'; 

        var vm = this; 

        vm.state =  {
            create:$state.includes('Root.WithOrg.Client.SubmitTicket')
        }; 

        vm.ticket =  {
            type: 0, 
            subject: '', 
            message: ''
        }; 
        vm.supportTypes = [];

        vm.supportSettings = supportSettings;
        vm.supportTypesResources = supportTypesResources;

        vm.submitTicket = submitTicket; 

        init(); 

        ///////

        function init() {
           getTypes(); 

           vm.isSaveButtonEnabled = true; 
        }

        function getTypes() {
            supportRepository.getTypes().then(function (response) {
                vm.supportTypes = response;               
            }); 
        }

        function submitTicket() {
            if (!vm.isSaveButtonEnabled) {
                return; 
            }

            vm.isSaveButtonEnabled = false; 

            supportRepository.submitTicket(vm.ticket).then(function (result) {
                    notifySrv.success('support.success'); 

                    $state.go('Root.WithOrg.Client.Wall.Item.Feed',  {
                        type:'All'
                    }); 
                }, errorHandler.handleErrorMessage); 
        }
     }
})(); 