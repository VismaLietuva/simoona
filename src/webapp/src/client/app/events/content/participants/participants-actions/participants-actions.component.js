(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventParticipantsActions', {
            bindings: {
                event: '=',
                isAdmin: '='
            },
            templateUrl: 'app/events/content/participants/participants-actions/participants-actions.html',
            controller: eventParticipantsActionsController,
            controllerAs: 'vm'
        });

    eventParticipantsActionsController.$inject = [
        'eventRepository',
        'notifySrv',
        'errorHandler',
        'Analytics',
        'attendStatus'
    ];

    function eventParticipantsActionsController(eventRepository, notifySrv, errorHandler, Analytics, attendStatus) {
        /* jshint validthis: true */
        var vm = this;
        vm.isEventFinished = !hasDatePassed(vm.event.endDate);

        vm.resetParticipantList = resetParticipantList;
        vm.exportParticipantsData = exportParticipantsData;

        /////////

        function resetParticipantList() {
            Analytics.trackEvent('Events', 'Reset participant list', 'event: ' + vm.event.id);
            eventRepository.resetParticipantList(vm.event.id).then(function() {
                vm.event.participants = [];
                vm.event.participantsCount = 0;
                vm.event.participatingStatus = attendStatus.NotAttending;
                if (!!vm.event.options.length) {
                    angular.forEach(vm.event.options, function(option) {
                        option.participants = [];
                    });
                }

                notifySrv.success('events.resetParticipantListSuccessfullMessage');
            }, errorHandler.handleErrorMessage);
        }

        function exportParticipantsData() {
            Analytics.trackEvent('Events', 'Export participant list', 'event: ' + vm.event.id);
            eventRepository.exportParticipants(vm.event.id).then(function(response) {
                var file = new Blob([response.data], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;'
                });
                saveAs(file, 'participants.xlsx');
            }, handleErrorMessage);
        }

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }

        function handleErrorMessage(error) {
            var decodedString = String.fromCharCode.apply(null, new Uint8Array(error.data));
            var errorObject = JSON.parse(decodedString);
            error.data.message = errorObject['message'];
            errorHandler.handleErrorMessage(error);
        }
    }
})();
