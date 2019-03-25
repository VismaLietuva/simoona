(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceEventJoin', {
            bindings: {
                event: '=',
                isDetails: '=',
                isAddColleague: '='
            },
            templateUrl: 'app/events/join/join.html',
            controller: eventJoinController,
            controllerAs: 'vm'
        });

    eventJoinController.$inject = [
        '$state',
        '$uibModal',
        'eventRepository',
        'notifySrv',
        'errorHandler',
        'authService',
        'eventParticipantsService',
        'Analytics'
    ];

    function eventJoinController($state, $uibModal, eventRepository, notifySrv, errorHandler,
        authService, eventParticipantsService, Analytics) {
        /* jshint validthis: true */
        var vm = this;

        vm.enableAction = true;
        vm.joinEvent = joinEvent;
        vm.leaveEvent = leaveEvent;
        vm.hasDatePassed = hasDatePassed;

        ////////
        function joinEvent(eventId) {
            if (vm.enableAction) {
                if (canJoinEvent()) {
                    vm.enableAction = false;
                    Analytics.trackEvent('Events join', 'isAddColleague: ' + vm.isAddColleague, 'isDetails: ' + vm.isDetails);

                    eventRepository.getEventOptions(eventId).then(function(responseEvent) {
                        vm.event.maxChoices = responseEvent.maxOptions;
                        vm.event.availableOptions = responseEvent.options;
                        if (!vm.event.availableOptions.length && !vm.isAddColleague) {
                            var selectedOptions = [];

                            eventRepository.joinEvent(eventId, selectedOptions).then(function() {
                                handleEventJoin();
                            }, function(error) {
                                vm.enableAction = true;
                                errorHandler.handleErrorMessage(error);
                            });
                        } else {
                            openOptionsModal();
                        }
                    });
                }
            }
        }

        function leaveEvent(eventId) {
            if (vm.enableAction) {
                if (canLeaveEvent()) {
                    vm.enableAction = false;

                    eventRepository.leaveEvent(eventId, authService.identity.userId).then(function() {
                        removeCurrentUser();
                    }, function(error) {
                        var errorActions = {
                            repeat: removeCurrentUser
                        };

                        vm.enableAction = true;

                        errorHandler.handleError(error, errorActions);
                    });
                }
            }
        }

        function removeCurrentUser() {
            vm.enableAction = true;
            vm.event.participantsCount--;
            vm.event.isParticipating = false;

            if (vm.isDetails || vm.isAddColleague) {
                var currentUserId = authService.identity.userId;

                eventParticipantsService.removeParticipant(vm.event.participants, currentUserId);
                eventParticipantsService.removeParticipantFromOptions(vm.event.options, currentUserId);
            }

            notifySrv.success('events.leaveEvent');
        }

        function handleEventJoin() {
            if (vm.isDetails) {
                eventRepository.getEventDetails(vm.event.id).then(function(response) {
                    angular.copy(response, vm.event);

                    vm.event.options = response.options;
                    vm.event.participants = response.participants;
                });
            } else {
                vm.event.isParticipating = true;
                vm.event.participantsCount++;
            }

            vm.enableAction = true;

            notifySrv.success('events.joinedEvent');
        }

        function openOptionsModal() {
            vm.enableAction = true;

            $uibModal.open({
                templateUrl: 'app/events/join/join-options/join-options.html',
                controller: 'eventJoinOptionsController',
                controllerAs: 'vm',
                resolve: {
                    event: function() {
                        return vm.event;
                    },
                    isDetails: function() {
                        return vm.isDetails;
                    },
                    isAddColleague: function() {
                        return vm.isAddColleague;
                    }
                }
            });
        }

        function canJoinEvent() {
            if (vm.event.date) {
                vm.event.startDate = vm.event.date;
            }

            if (!hasDatePassed(vm.event.startDate)) {
                notifySrv.error('errorCodeMessages.messageEventJoinStartedOrFinished');
                return false;
            } else if (!hasDatePassed(vm.event.registrationDeadlineDate)) {
                notifySrv.error('events.eventJoinRegistrationDeadlinePassed');
                return false;
            }

            return true;
        }

        function canLeaveEvent() {
            if (!hasDatePassed(vm.event.registrationDeadlineDate)) {
                notifySrv.error('events.eventLeaveRegistrationDeadlinePassed');
                return false;
            }

            return true;
        }

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }

    }
})();
