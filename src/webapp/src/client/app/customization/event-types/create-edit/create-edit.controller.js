(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.EventTypes')
        .controller('eventTypesCreateController', eventTypesCreateController);

    eventTypesCreateController.$inject = [
        '$rootScope',
        '$state',
        '$stateParams',
        'eventTypesRepository',
        'notifySrv',
        'errorHandler'
    ];

    function eventTypesCreateController($rootScope, $state, $stateParams, eventTypesRepository, notifySrv, errorHandler) {
        var vm = this;
        var listState = 'Root.WithOrg.Admin.Customization.EventTypes.List';

        vm.singleJoinGroups = [];

        vm.eventType = {};
        vm.onEditOriginalName = '';
        vm.states = {
            isAdd: $state.includes('Root.WithOrg.Admin.Customization.EventTypes.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Customization.EventTypes.Edit')
        };
        vm.isLoading = vm.states.isAdd ? false : true;

        $rootScope.pageTitle = vm.states.isAdd ? 'customization.createEventType' : 'customization.editEventType';

        vm.createEventType = createEventType;
        vm.updateEventType = updateEventType;
        vm.saveEventType = saveEventType;
        vm.deleteEventType = deleteEventType;

        init();

        ///////////

        function init() {
            if ($stateParams.id) {
                eventTypesRepository.getEventType($stateParams.id).then(function (response) {
                    vm.eventType = response;
                    vm.onEditOriginalName = response.name;
                    vm.isLoading = false;
                }, function (error) {
                    errorHandler.handleErrorMessage(error);
                    $state.go(listState);
                });
            }

            eventTypesRepository.getSingleJoinGroups().then(function(response) {
                vm.singleJoinGroups = response;
            });
        }

        function createEventType() {
            if (!vm.eventType.isSingleJoin) {
                vm.eventType.isSingleJoin = false;
            }

            return eventTypesRepository.createEventType(vm.eventType);
        }

        function updateEventType() {
            return eventTypesRepository.updateEventType(vm.eventType);
        }

        function saveEventType(method) {
            method().then(function (response) {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
        }

        function deleteEventType(id) {
            eventTypesRepository.deleteEventType(id).then(function(response) {
                $state.go(listState);
            }, errorHandler.handleErrorMessage);
        }
    }
})();