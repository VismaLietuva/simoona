(function () {
    'use strict'; 

    angular.module('simoonaApp.Recommendation')
        .constant('recommendationSettings', {
            subjectLength: 300,
            messageLength: 5000
        })
        .controller('recommendationController', recommendationController); 

        recommendationController.$inject = [
        '$state', 
        '$rootScope',    
        'recommendationRepository', 
        'notifySrv', 
        'errorHandler',
        'recommendationSettings',
       ]; 

    function recommendationController($state, $rootScope, recommendationRepository, notifySrv, errorHandler, recommendationSettings) {
        
        $rootScope.pageTitle = 'friendRecommendation.submitTicket'; 

        var vm = this; 

        vm.state =  {
            create:$state.includes('Root.WithOrg.Client.FriendRecommendation')
        }; 

        vm.ticket =  {
            name: '', 
            lastName: '',
            contact:'',
            message:''
        }; 


        vm.recommendationSettings = recommendationSettings;
       
        vm.submitTicket = submitTicket; 

        init(); 

        ///////

        function init() {
        
           vm.isSaveButtonEnabled = true; 
        }

        function submitTicket() {
            if (!vm.isSaveButtonEnabled) {
                return; 
            }

            vm.isSaveButtonEnabled = false; 

            recommendationRepository.submitTicket(vm.ticket).then(function (result) {
                    notifySrv.success('support.success'); 

                    $state.go('Root.WithOrg.Client.Wall.Item.Feed',  {
                        type:'All'
                    }); 
                }, errorHandler.handleErrorMessage); 
        }
     }
})(); 