(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.JobTitles')
        .controller('jobTitlesController', jobTitlesController);

    jobTitlesController.$inject = [
        '$rootScope',
        'jobTitlesRepository',
        'errorHandler'
    ];

    function jobTitlesController($rootScope, jobTitlesRepository, errorHandler) {
        var vm = this;

        $rootScope.pageTitle = 'customization.jobTitles';
        
        vm.isLoading = true;
        vm.jobTitles = [];

        init();

        ///////////

        function init() {
            jobTitlesRepository.getJobTitles().then(function(response) {
                vm.jobTitles = response;
                vm.isLoading = false;
            }, errorHandler.handleErrorMessage);
        }
    }
})();
