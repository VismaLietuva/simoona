(function () {
    'use strict';

    angular.module('simoonaApp.Project')
        .controller('projectListController', projectListController);

    projectListController.$inject = [
        '$rootScope',
        'projectRepository',
        'errorHandler',
        '$scope',
        'lodash'
    ];

    function projectListController($rootScope, projectRepository, errorHandler, $scope, lodash) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'projects.projectListTitle';

        vm.projects = [];

        vm.filter = {
            page: 1
        };

        vm.pageSize = 10;
        vm.isLoading = true;

        init();

        vm.changePage = changePage;
        vm.onListFilter = onListFilter;
        vm.Math = Math;

        /////////

        function init() 
        {
            projectRepository.getProjectList().then(function (response)
            {
                vm.projects = response;
                vm.isLoading = false;
            }, errorHandler.handleErrorMessage);
        }

        function changePage(page) {
            vm.filter.page = page;
        }

        function onListFilter() {
            vm.filter.page = 1;
        }
    }
})();