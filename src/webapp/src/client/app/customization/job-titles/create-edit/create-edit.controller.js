(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.JobTitles')
        .controller('jobTitlesCreateController', jobTitlesCreateController);

    jobTitlesCreateController.$inject = [
        '$rootScope',
        '$state',
        '$stateParams',
        'jobTitlesRepository',
        'notifySrv',
        'errorHandler'
    ];

    function jobTitlesCreateController($rootScope, $state, $stateParams, jobTitlesRepository, notifySrv, errorHandler) {
        var vm = this;
        var listState = 'Root.WithOrg.Admin.Customization.JobTitles.List';

        vm.jobTitleData  = {};
        vm.onEditOriginalName = '';
        vm.states = {
            isAdd: $state.includes('Root.WithOrg.Admin.Customization.JobTitles.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Customization.JobTitles.Edit')
        };
        vm.isLoading = vm.states.isAdd ? false : true;

        $rootScope.pageTitle = vm.states.isAdd ? 'customization.createJobTitle' : 'customization.editJobTitle';

        vm.createJobTitle = createJobTitle;
        vm.updateJobTitle = updateJobTitle;
        vm.saveJobTitle = saveJobTitle;
        vm.deleteJobTitle = deleteJobTitle;

        init();

        ///////////

        function init() {
			var jobTitleId = $stateParams.id;
            if (jobTitleId) {
                jobTitlesRepository.getJobTitle(jobTitleId).then(function (response) {
                    vm.jobTitleData  = response;
                    vm.onEditOriginalName = response.title;
                    vm.isLoading = false;
                }, function (error) {
                    errorHandler.handleErrorMessage(error);
                    $state.go(listState);
                });
            }
        }

        function createJobTitle() {
            return jobTitlesRepository.createJobTitle(vm.jobTitleData );
        }

        function updateJobTitle() {
            return jobTitlesRepository.updateJobTitle(vm.jobTitleData );
        }

        function saveJobTitle(method) {
            method().then(function () {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
        }

        function deleteJobTitle(id) {
            jobTitlesRepository.deleteJobTitle(id).then(function() {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
        }

    }
})();