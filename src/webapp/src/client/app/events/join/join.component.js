(function () {
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
            Idle: 3,
            AttendingVirtually: 4
        });

    eventJoinController.$inject = [
        '$uibModal',
        'eventRepository',
        'eventService',
        'notifySrv',
        'errorHandler',
        'authService',
        'Analytics',
        'attendStatus'
    ];

    function eventJoinController(
        $uibModal,
        eventRepository,
        eventService,
        notifySrv,
        errorHandler,
        authService,
        Analytics,
        attendStatus) {
        /* jshint validthis: true */
        var vm = this;

        vm.attendStatus = attendStatus;
        vm.enableAction = true;
        vm.joinEvent = joinEvent;
        vm.leaveEvent = leaveEvent;
        vm.updateEventStatus = updateEventStatus;
        vm.hasDatePassed = hasDatePassed;
        vm.openJoinCommentModal = openJoinCommentModal;
        vm.closeModal = closeModal;

        vm.isAttendingEvent = isAttendingEvent;
        vm.isVirtualParticipantsCapacityReached = isVirtualParticipantsCapacityReached;
        vm.isParticipantsCapacityReached = isParticipantsCapacityReached;

        ////////
        function joinEvent(eventId, attendingStatus) {
            if (!vm.enableAction) {
                return;
            }

            if (!canJoinEvent()) {
                return;
            }

            vm.enableAction = false;
            Analytics.trackEvent('Events join', 'isAddColleague: ' + vm.isAddColleague, 'isDetails: ' + vm.isDetails);

            eventRepository.getEventOptions(eventId).then(function (responseEvent) {
                vm.event.maxChoices = responseEvent.maxOptions;
                vm.event.availableOptions = responseEvent.options;
                if (!vm.event.availableOptions.length && !vm.isAddColleague) {
                    var selectedOptions = [];

                    var comment = "";
                    eventRepository.joinEvent(eventId, selectedOptions, attendingStatus, comment).then(function () {
                        handleEventJoin(attendingStatus);
                        notifySrv.success('events.joinedEvent');
                    }, function (error) {
                        vm.enableAction = true;
                        errorHandler.handleErrorMessage(error);
                    });
                } else {
                    openOptionsModal(attendingStatus);
                }
            });
        }

        function leaveEvent(eventId) {
            if (vm.enableAction) {
                if (canLeaveEvent()) {
                    vm.enableAction = false;
                    var comment = "";
                    eventRepository.leaveEvent(eventId, authService.identity.userId, comment).then(function () {
                        handleEventLeave();
                    }, function (error) {
                        var errorActions = {
                            repeat: handleEventLeave
                        };
                        vm.enableAction = true;

                        errorHandler.handleError(error, errorActions);
                    });
                }
            }
        }

        function isVirtualParticipantsCapacityReached() {
            return getVirtualParticipantCount() >= vm.event.maxVirtualParticipants;
        }

        function isParticipantsCapacityReached() {
            return getParticipantCount() >= vm.event.maxParticipants;
        }

        function getParticipantCount() {
            return vm.event.participantsCount !== undefined ?
                vm.event.participantsCount :
                countAttendingParticipants();
        }

        function getVirtualParticipantCount() {
            return vm.event.virtualParticipantsCount !== undefined ?
                vm.event.virtualParticipantsCount :
                countVirtuallyAttendingParticipants();
        }

        function countVirtuallyAttendingParticipants() {
            return eventService.countParticipants(vm.event, attendStatus.AttendingVirtually);
        }

        function countAttendingParticipants() {
            return eventService.countParticipants(vm.event, attendStatus.Attending);
        }

        function isAttendingEvent() {
            return vm.event.participatingStatus == vm.attendStatus.Attending ||
                   vm.event.participatingStatus == vm.attendStatus.AttendingVirtually;
        }

        function updateEventStatus(eventId, changeToAttendStatus, comment) {
            if (vm.enableAction) {
                eventRepository.updateAttendStatus(changeToAttendStatus, comment, eventId).then(function () {
                    handleEventJoin(changeToAttendStatus);

                    if (changeToAttendStatus == attendStatus.MaybeAttending) {
                        notifySrv.success('events.maybeJoiningEvent');
                    } else if (changeToAttendStatus == attendStatus.NotAttending) {
                        notifySrv.success('events.notJoiningEvent');
                    }

                }, function (error) {
                    vm.enableAction = true;
                    errorHandler.handleErrorMessage(error);
                });
            }
        }

        function openJoinCommentModal(changeToAttendStatus) {
            $uibModal.open({
                templateUrl: 'app/events/join-comment/join-comment.html',
                controller: 'joinCommentController',
                controllerAs: 'vm',
                resolve: {
                    event: function () {
                        return vm.event;
                    },
                    updateEventStatus: function () {
                        return vm.updateEventStatus;
                    },
                    changeToAttendStatus: function () {
                        return changeToAttendStatus;
                    }
                }
            });
        }

        function closeModal() {
            $uibModalInstance.close();
        }

        function handleEventLeave() {
            eventRepository.getEventDetails(vm.event.id).then(function (response) {
                updateLoadedEventDataFromDetails(response);
            });
            vm.enableAction = true;
            notifySrv.success('events.leaveEvent');
        }

        function handleEventJoin(attendStatus) {
            if (vm.isDetails) {
                eventRepository.getEventDetails(vm.event.id).then(function (response) {
                    updateLoadedEventDataFromDetails(response);
                    console.log('requst');
                });
            } else {
                vm.event.participatingStatus = attendStatus;
                increaseParticipantCount(attendStatus);
            }

            vm.enableAction = true;
        }

        function increaseParticipantCount(attendStatus) {
            switch (attendStatus) {
                case vm.attendStatus.Attending:
                    vm.event.participantsCount++;
                    break;
                case vm.attendStatus.AttendingVirtually:
                    vm.event.virtualParticipantsCount++;
                    break;
                default:
                    console.error('Should not be used with ' + attendStatus);
            }
        }

        function updateLoadedEventDataFromDetails(response) {
            angular.copy(response, vm.event);
            vm.event.options = response.options;
            vm.event.participants = response.participants;
            vm.event.participantsCount = countAttendingParticipants();
            vm.event.virtualParticipantsCount = countVirtuallyAttendingParticipants();
        }

        function openOptionsModal(attendStatus) {
            vm.enableAction = true;

            $uibModal.open({
                templateUrl: 'app/events/join/join-options/join-options.html',
                controller: 'eventJoinOptionsController',
                controllerAs: 'vm',
                resolve: {
                    event: function () {
                        return vm.event;
                    },
                    isDetails: function () {
                        return vm.isDetails;
                    },
                    isAddColleague: function () {
                        return vm.isAddColleague;
                    },
                    isChangeOptions: function () {
                        return false;
                    },
                    selectedAttendStatus: function() {
                        return attendStatus;
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
