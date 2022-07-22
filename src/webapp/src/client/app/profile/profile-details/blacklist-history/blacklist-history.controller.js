(function () {
    'use strict';

    angular
        .module('simoonaApp.Profile')
        .constant('blacklistStatus', {
            active: 0,
            canceled: 1,
            expired: 2
        })
        .directive('aceBlacklistHistory', blacklistHistory);

    function blacklistHistory() {
        var directive = {
            templateUrl: 'app/profile/profile-details/blacklist-history/blacklist-history.html',
            restrict: 'E',
            replace: true,
            scope: {
            },
            controller: blacklistHistoryController,
            controllerAs: 'vm',
            bindToController: true,
        }

        return directive;
    }

    blacklistHistoryController.$inject = [
        '$stateParams',
        'profileRepository',
        'notifySrv',
        'blacklistStatus'
    ];

    function blacklistHistoryController($stateParams, profileRepository, notifySrv, blacklistStatus) {
        var vm = this;

        vm.blacklistEntries;
        vm.blacklistHistoryExpanded = false;
        vm.showButtonTranslation = vm.blacklistHistoryExpanded ? 'applicationUser.hideBlacklistHistory' : 'applicationUser.showBlacklistHistory';
        vm.isLoading = true;

        vm.toggleHistory = toggleHistory;
        vm.getStatusTranslation = getStatusTranslation;

        function toggleHistory() {
            vm.blacklistHistoryExpanded = !vm.blacklistHistoryExpanded;
            vm.showButtonTranslation = vm.blacklistHistoryExpanded ? 'applicationUser.hideBlacklistHistory' : 'applicationUser.showBlacklistHistory';

            if (vm.isLoading) {
                loadBlacklistHistory();
            }
        }

        function loadBlacklistHistory() {
            profileRepository
                .getBlacklistHistory({userId: $stateParams.id})
                .then(function(response) {
                    vm.blacklistEntries = response;
                    vm.isLoading = false;
                }, function() {
                    notifySrv.error('errorCodeMessages.messageError');
                });
        }

        function getStatusTranslation(status) {
            switch(status) {
                case blacklistStatus.canceled:
                    return 'applicationUser.blacklistHistoryStatusCanceled';
                case blacklistStatus.expired:
                    return 'applicationUser.blacklistHistoryStatusExpired';
                default:
                    console.error(`Status ${status} should not be present in blacklist history`);
                    return status;
            }
        }
    }
})();
