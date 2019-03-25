(function () {
    'use strict';
    angular
        .module('simoonaApp.Committee')
        .controller('CommitteeController', CommitteeController);

    CommitteeController.$inject = [
        '$scope',
        'committeeRepository',
        '$uibModal',
        'notifySrv',
        '$rootScope',
        'committees',
        'localeSrv'
    ];

    function CommitteeController($scope, committeeRepository, $uibModal,
     notifySrv, $rootScope, committees, localeSrv) {
        $rootScope.pageTitle = 'committee.committeesList';

        $scope.committees = committees;

        $scope.getKudosCommitteeId = getKudosCommitteeId;
        $scope.addSuggestionModal = addSuggestionModal;
        $scope.getCommitteesList = getCommitteesList;
        $scope.setFocusedCommittee = setFocusedCommittee;
        $scope.getSuggestions = getSuggestions;
        $scope.deleteButtonClick = deleteButtonClick;
        $scope.deleteItem = deleteItem;
        $scope.deleteSuggestion = deleteSuggestion;
        $scope.onNewCommitteeButtonClick = onNewCommitteeButtonClick;
        $scope.onEditCommitteeButtonClick = onEditCommitteeButtonClick;

        $scope.edit = false;
        $scope.focusedCommittee = {};
        $scope.emailsString = 'mailTo:';
        $scope.currentCommitteeIndex = 0;

        $scope.suggestions = [];

        $scope.filter = {
            includeProperties: 'Members, Delegates, Leads',
            orderBy: 'Name'
        };

        $scope.kudosCommitteeId = 0;

        $scope.getKudosCommitteeId();

        function getKudosCommitteeId() {
            committeeRepository.getKudosCommitteeId().then(function (response) {
                $scope.kudosCommitteeId = response.id;
            });
        }

        function addSuggestionModal() {
            if (!$scope.focusedCommittee) {
                return 0;
            }

            $scope.edit = false;
            $scope.committee = $scope.focusedCommittee;
            $uibModal.open({
                templateUrl: 'app/committee/committee-suggestion-manage.html',
                controller: 'CommitteeSuggestionModalController',
                scope: $scope,
                backdrop: 'static'
            });
        }

        function getCommitteesList(filter) {
            committeeRepository.getAll(filter).then(function (response) {
                $scope.committees = response;
                $scope.focusedCommittee = $scope.committees[$scope.currentCommitteeIndex];
                $scope.setFocusedCommittee($scope.committees[$scope.currentCommitteeIndex]);
            });
        }

        function setFocusedCommittee(committee) {
            $scope.emailsString = 'mailTo:';
            $scope.focusedCommittee = committee;
            $scope.currentCommitteeIndex = $scope.committees.indexOf(committee);
            for (var i = 0; i < committee.members.length; i++) {
                $scope.emailsString += (committee.members[i].email + ',');
            }

            $scope.emailsString = $scope.emailsString.slice(0, -1);
            $scope.getSuggestions();
        }

        function getSuggestions() {
            committeeRepository.getSuggestions($scope.focusedCommittee.id).then(function (response) {
                $scope.suggestions = response;
            });
        }

        function deleteButtonClick(id) {
            committeeRepository.delete(id).then(function (response) {
                $scope.getCommitteesList($scope.filter);
            });
        }

        function deleteItem(committee) {
            committeeRepository.deleteItem(committee.id).then(function () {
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted', {one: 'committee.committeeWord', two: committee.name}));
                $scope.getCommitteesList($scope.filter);
            });
        }

        function deleteSuggestion(suggestion) {
            committeeRepository.deleteSuggestion($scope.focusedCommittee.id, suggestion.id).then(function () {
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted', {one: 'committee.suggestionWord', two: suggestion.title}));
                $scope.getSuggestions();
            });
        }

        function onNewCommitteeButtonClick() {
            $scope.edit = false;
            $scope.committee = {};
            $uibModal.open({
                templateUrl: 'app/committee/committee-new-edit-modal.html',
                controller: 'newCommitteeModalController',
                scope: $scope,
                backdrop: 'static'
            });
        }

        function onEditCommitteeButtonClick(committee) {
            $scope.edit = true;
            $scope.committee = committee;
            $uibModal.open({
                templateUrl: 'app/committee/committee-new-edit-modal.html',
                controller: 'newCommitteeModalController',
                scope: $scope,
                backdrop: 'static'
            });
        }
    }
})();
