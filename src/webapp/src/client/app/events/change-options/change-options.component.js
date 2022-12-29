(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventOptionsChange', {
            bindings: {
                event: '<'
            },
            templateUrl: 'app/events/change-options/change-options.html',
            controller: eventOptionsChangeController,
            controllerAs: 'vm'
        });

    eventOptionsChangeController.$inject = [
        'eventRepository',
        '$uibModal'
    ];

    function eventOptionsChangeController(eventRepository, $uibModal) {
        var vm = this;

        vm.changeSelectedOptions = changeSelectedOptions;
        vm.isDeadline = isDeadline;


        function changeSelectedOptions() {
            eventRepository.getEventOptions(vm.event.id).then(function (responseEvent) {
                vm.event.maxChoices = responseEvent.maxOptions;
                vm.event.availableOptions = responseEvent.options;
                openOptionsModal();
            });
        }

        function openOptionsModal() {
            $uibModal.open({
                templateUrl: 'app/events/join/join-options/join-options.html',
                controller: 'eventJoinOptionsController',
                controllerAs: 'vm',
                resolve: {
                    event: function () {
                        return vm.event;
                    },
                    isChangeOptions: function () {
                        return true;
                    },
                    isDetails: function () {
                        return false;
                    },
                    isAddColleague: function () {
                        return false;
                    },
                    selectedAttendStatus: function() {
                        return undefined;
                    }
                }
            });
        }

        function isDeadline() {
            return moment.utc(vm.registrationDeadlineDate).local().isAfter();
        }
    }
})();
