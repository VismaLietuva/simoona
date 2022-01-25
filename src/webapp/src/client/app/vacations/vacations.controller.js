(function () {
    'use strict';

    angular
        .module('simoonaApp.Vacations')
        .controller('vacationsController', vacationsController);

    vacationsController.$inject = [
        '$rootScope',
        '$scope',
        'vacationsRepository',
        'authService'
    ];

    function vacationsController($rootScope, $scope, vacationsRepository, authService) {

        $rootScope.pageTitle = 'common.vacations';

        var vm = this;

        vm.allowEdit = authService.hasPermissions(['APPLICATIONUSER_ADMINISTRATION']);
        vm.isLoading = true;
        vm.isTranslating = false;

        vm.editableValue = vm.content;
        vm.editFieldEnabled = false;

        vm.enableEditor = enableEditor;
        vm.disableEditor = disableEditor;
        vm.editContent = editContent;

        init();

        //////////

        function init() {
            vacationsRepository.getVacationContent().then(function (response) {
                vm.content = response.content;
                vm.isLoading = false;
            }, function () {
                vm.isLoading = false;
            })
        }

        function editContent(editedContent) {
            vm.content = editedContent;

            vacationsRepository.setVacationContent(editedContent).then(function () {
                disableEditor();
            }, function () {
                enableEditor();
            });
        }

        function enableEditor() {
            vm.editFieldEnabled = true;
            vm.editableValue = vm.content;
        }

        function disableEditor() {
            vm.editFieldEnabled = false;
        }
    }
})();
