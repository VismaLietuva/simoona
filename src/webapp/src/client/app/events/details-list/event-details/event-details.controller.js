(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .controller('eventDetailsController', eventDetailsController);


    eventDetailsController.$inject = [
        'authService',
        'eventRepository',
        'kudosTypesRepository',
        '$stateParams',
        'eventSettings',
        'notifySrv'
    ];

    function eventDetailsController(authService, eventRepository, kudosTypesRepository, $stateParams, eventSettings, notifySrv) {
        var vm = this;

        vm.eventImageSize = {
            w: eventSettings.thumbWidth,
            h: eventSettings.thumbHeight
        };

        vm.filter = {
            appliedKudos: undefined,
            appliedConferences: undefined,
        }

        vm.settings = {
            projects: {
                itemsLength: 1,
                itemsMinLength: 1
            },
            conferences: {
                itemsLength: 1,
                itemsMinLength: 1
            }
        }

        vm.pageSize = 2;
        vm.page = 1;

        vm.setKudosTypes = setKudosTypes;
        vm.setConferenceTypes = setConferenceTypes;
        vm.changePage = changePage;

        loadKudosTypes();
        loadEventTypes();
        loadEventDetails();


        function loadEventDetails() {
            vm.isLoading = true;
            vm.pageContent = undefined;
            eventRepository.getExtensiveEventDetails($stateParams.id, vm.filter.appliedKudos, vm.filter.appliedConferences).then(function (result) {
                vm.eventDetails = result;
                loadPage();
                vm.isLoading = false;
                vm.loadControls = vm.eventDetails.extensiveParticipants.length || false;
            }, function () {
                notifySrv.error('errorCodeMessages.messageError');
            })
        }

        function loadKudosTypes() {
            kudosTypesRepository.getKudosTypes().then(function(result) {
                vm.kudosTypes = result;
            }, function() {
                notifySrv.error('errorCodeMessages.messageError');
            })
        }

        function loadEventTypes() {
            eventRepository.getEventTypes().then(
                function (result) {
                    vm.eventTypes = result;
                }, function () {
                    notifySrv.error("errorCodeMessages.messageError");
                }
            );
        }

        function setKudosTypes(types) {
            vm.filter.appliedKudos = types;

            loadEventDetails();
        }

        function setConferenceTypes(types) {
            vm.filter.appliedConferences = types;

            loadEventDetails();
        }

        function changePage(page) {
            vm.page = page;

            loadPage();
        }

        function loadPage() {
            vm.pageContent = vm.eventDetails
                .extensiveParticipants
                .slice((vm.page - 1) * vm.pageSize, (vm.page - 1) * vm.pageSize + vm.pageSize);
        }
    }
})();
