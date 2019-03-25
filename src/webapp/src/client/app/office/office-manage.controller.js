(function () {
    "use strict";

    angular.module('simoonaApp.Office')
        .controller('officeModifyController', officeModifyController);

    officeModifyController.$inject = ['$scope', '$state', '$rootScope', 'officeRepository', 'office', 'notifySrv', '$translate', 'localeSrv'];

    function officeModifyController($scope, $state, $rootScope, officeRepository, office, notifySrv, $translate, localeSrv) {

        init();

        function init() {

            $scope.onSave = onSave;

            if ($state.current.name === 'Root.WithOrg.Admin.Offices.Edit') {
                $scope.titleEdit = false;
                $scope.titleCreate = true;
                $rootScope.pageTitle = 'office.editOffice';
            }
            if ($state.current.name === 'Root.WithOrg.Admin.Offices.Create') {
                $scope.titleCreate = false;
                $scope.titleEdit = true;
                $rootScope.pageTitle = 'office.createOffice';
            }

            $scope.state = $state.current.name;
            $scope.office = office;
        }

        function onSave() {
            $scope.errors = [];

            if ($state.current.name === 'Root.WithOrg.Admin.Offices.Create') {
                officeRepository.create($scope.office).then(
                    function (data) {
                        $state.go('^.List');
                        notifySrv.success(localeSrv.formatTranslation('common.messageEntityCreated', {one: 'office.entityName', two: $scope.office.name}));
                    },
                    function (response) {
                        $scope.errors = response.data;
                    });
            }
            if ($state.current.name === 'Root.WithOrg.Admin.Offices.Edit') {
                officeRepository.update($scope.office).then(
                    function () {
                        $state.go('^.List');
                        notifySrv.success(localeSrv.formatTranslation('common.messageEntityChanged', {one: 'office.entityName', two: $scope.office.name}));
                    },
                    function (response) {
                        $scope.errors = response.data;
                    });
            }
        }
    };
})();
