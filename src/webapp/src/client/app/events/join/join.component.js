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
        })
        .constant("attendStatus", {
            NotAttending: 0,
            Attending: 1,
            MaybeAttending: 2,
            Idle: 3
        });

    eventJoinController.$inject = [
        '$uibModal',
        'eventRepository',
        'notifySrv',
        'errorHandler',
        'authService',
        'eventParticipantsService',
        'Analytics',
        'attendStatus'
    ];

    function eventJoinController($uibModal, eventRepository, notifySrv, errorHandler,
        authService, eventParticipantsService, Analytics, attendStatus) {
        /* jshint validthis: true */
        var vm = this;

        vm.attendStatus = attendStatus;
        vm.enableAction = true;
        vm.joinEvent = joinEvent;
        vm.leaveEvent = leaveEvent;
        vm.maybeParticipating = maybeParticipating;
        vm.notParticipating = notParticipating;
        vm.hasDatePassed = hasDatePassed;
        vm.openLeaveCommentModal = openLeaveCommentModal;
        vm.closeModal = closeModal;


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

                            var comment = "";
                            eventRepository.joinEvent(eventId, selectedOptions, attendStatus.Attending, comment).then(function() {
                                handleEventJoin();
                                notifySrv.success('events.joinedEvent');
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

        function leaveEvent(eventId, comment) {
            if (vm.enableAction) {
                if (canLeaveEvent()) {
                    vm.enableAction = false;
                    eventRepository.leaveEvent(eventId, authService.identity.userId, comment).then(function() {
                        handleEventLeave();
                    }, function(error) {
                        var errorActions = {
                            repeat: handleEventLeave
                        };
                        vm.enableAction = true;

                        errorHandler.handleError(error, errorActions);
                    });
                }
            }
        }

        function maybeParticipating(eventId) {
            if (vm.enableAction) {
                var comment = "";
                eventRepository.updateAttendStatus(attendStatus.MaybeAttending, comment, eventId).then(function() {
                    handleEventJoin();
                    notifySrv.success('events.maybeJoiningEvent');
                }, function(error) {
                    vm.enableAction = true;
                    errorHandler.handleErrorMessage(error);
                });
            }
        }

        function notParticipating(eventId, comment) {
            if (vm.enableAction) {
                eventRepository.updateAttendStatus(attendStatus.NotAttending, comment, eventId).then(function() {
                    handleEventJoin();
                    notifySrv.success('events.notJoiningEvent');
                }, function(error) {
                    vm.enableAction = true;
                    errorHandler.handleErrorMessage(error);
                });
            }
        }

        function openLeaveCommentModal() {
            $uibModal.open({
                templateUrl: 'app/events/leave/leave-event.html',
                controller: 'eventLeaveController',
                controllerAs: 'vm',
                resolve: {
                    event: function() {
                        return vm.event;
                    },
                    leaveEvent: function() {
                        return vm.leaveEvent;
                    },
                    notInterested: function() {
                        return vm.notParticipating;
                    }
                }
              });
        }

        function closeModal() {
            $uibModalInstance.close();
        }

        function handleEventLeave() {
            eventRepository.getEventDetails(vm.event.id).then(function(response) {
                angular.copy(response, vm.event);

                vm.event.options = response.options;
                vm.event.participants = response.participants;
                vm.event.participantsCount = recalculateJoinedParticipants();
            });
            vm.enableAction = true;
            notifySrv.success('events.leaveEvent');
        }

        function handleEventJoin() {
            if (vm.isDetails) {
                eventRepository.getEventDetails(vm.event.id).then(function(response) {
                    angular.copy(response, vm.event);

                    vm.event.options = response.options;
                    vm.event.participants = response.participants;
                    vm.event.participantsCount = recalculateJoinedParticipants();
                });
            } else {
                vm.event.participatingStatus = attendStatus.Attending;
                vm.event.participantsCount++;
            }

            vm.enableAction = true;
        }

        function recalculateJoinedParticipants() {
            var participantsCount = 0;
            vm.event.participants.forEach(function(participant){
                if (participant.attendStatus == attendStatus.Attending) {
                    participantsCount++;
                }
            });
            return participantsCount;
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
