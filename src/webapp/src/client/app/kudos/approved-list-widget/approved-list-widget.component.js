(function() {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosApprovedList', {
            templateUrl: 'app/kudos/approved-list-widget/approved-list-widget.html',
            controller: kudosApprovedListController,
            controllerAs: 'vm'
        });

    kudosApprovedListController.$inject = [
        '$stateParams',
        'kudosFactory',
        'authService',
        'kudosLogSettings',
        'errorHandler'
    ];

    function kudosApprovedListController($stateParams, kudosFactory, authService, kudosLogSettings, errorHandler) {
        /*jshint validthis: true */
        var vm = this;
        var userId = $stateParams.userId || authService.identity.userId;

        vm.getApprovedKudosList = getApprovedKudosList;
        vm.kudosLogSettings = kudosLogSettings;
        vm.isKudosLogsLoading = true;
        
        vm.filter = {
            page: 1
        };

        init();

        //////////

        function init() {
            getApprovedKudosList();
        }

        function getApprovedKudosList() {
            vm.isKudosLogsLoading = true;
            kudosFactory.getApprovedKudosList(userId, vm.filter).then(function(response) {
                vm.approvedList = response.pagedList;
                vm.totalItems = response.itemCount;
                vm.perPage = response.pageSize;
                vm.isKudosLogsLoading = false;
            }, function(error) {
                vm.isKudosLogsLoading = false;
                errorHandler.handleErrorMessage(error);
            });
        }
    }
}());
