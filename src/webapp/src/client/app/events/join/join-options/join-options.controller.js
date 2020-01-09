(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('inputTypes', {
            checkbox: 'checkbox',
            radio: 'radio'
        })
        .controller('eventJoinOptionsController', eventJoinOptionsController);

    eventJoinOptionsController.$inject = [
        '$state',
        '$uibModalInstance',
        'inputTypes',
        'authService',
        'errorHandler',
        'eventRepository',
        '$translate',
        'notifySrv',
        'event',
        'isDetails',
        'isAddColleague',
        'localeSrv',
        'lodash',
        'optionRules'
    ];

    function eventJoinOptionsController($state, $uibModalInstance, inputTypes, authService, errorHandler,
        eventRepository, $translate, notifySrv, event, isDetails, isAddColleague, localeSrv, lodash, optionRules) {
        /* jshint validthis: true */
        var vm = this;

        vm.options = event.availableOptions;
        vm.inputType = null;
        vm.isAddColleague = isAddColleague;
        vm.participants = [];
        vm.selectedOptions = [];
        vm.messageMaximumOptions = localeSrv.formatTranslation('events.eventMaximumOptions', {one: event.maxChoices});
        vm.isActionDisabled = false;

        vm.joinEvent = joinEvent;
        vm.closeModal = closeModal;
        vm.selectOption = selectOption;
        vm.isOptionsJoinAvailable = isOptionsJoinAvailable;
        vm.getUserForAutoComplete = getUserForAutoComplete;
        vm.isTooManyOptionsSelected = isTooManyOptionsSelected;
        vm.isOptionSelected = isOptionSelected;

        init();

        //////

        function init() {
            if (event.maxChoices > 1) {
                vm.inputType = inputTypes.checkbox;
            } else {
                vm.inputType = inputTypes.radio;
            }

            eventRepository.getUserForAutoComplete(authService.identity.userName, event.id).then(function(response) {
                for (var i = 0; response.length > i; i++) {
                    if (response[i].id === authService.identity.userId) {
                        vm.participants.push(response[i]);
                    }
                }
            });
        }

        function getUserForAutoComplete(search) {
            return eventRepository.getUserForAutoComplete(search, event.id);
        }

        function selectOption(optionId) {
            if (vm.inputType === inputTypes.checkbox) {
                var index = vm.selectedOptions.indexOf(optionId);
                if (index > -1) {
                    vm.selectedOptions.splice(index, 1); 
                } else {
                    if (isOnlySingleSelectable(optionId)) {
                        vm.selectedOptions.length = 0;
                    } else {
                        uncheckSingleSelectable();
                    }
                    vm.selectedOptions.push(optionId);
                }
            } else {
                vm.selectedOptions = [optionId];
            }
        }

        function isOnlySingleSelectable(selectedOptionId) {
            var option = vm.options.find(x => x.id === selectedOptionId);

            return option.rule === optionRules.ignoreSingleJoin;
        }

        function uncheckSingleSelectable() {
            var single = vm.options.find(op => op.rule === optionRules.ignoreSingleJoin);
            var index = vm.selectedOptions.findIndex(op => single.id === op);
            
            if (index > -1) {
                vm.selectedOptions.splice(index, 1);
            }
        }

        function isOptionSelected(optionId) {
            return vm.selectedOptions.indexOf(optionId) > -1;
        }

        function joinEvent() {
            vm.isActionDisabled = true;

            if (vm.selectedOptions.length > event.maxChoices) {
                handleErrorMessage($translate.instant('events.maxOptionsError') + ' ' + event.maxChoices);
            } else if (!vm.selectedOptions.length && event.options.length) {
                handleErrorMessage('errorCodeMessages.messageNotEnoughOptions');
            } else if (vm.isAddColleague && !vm.participants.length) {
                handleErrorMessage('events.noParticipantsError');
            } else if (vm.isAddColleague && vm.participants.length + event.participants.length > event.maxParticipants) {
                var participants = event.maxParticipants - event.participants.length;
                handleErrorMessage($translate.instant('events.maxParticipantsError') + ' ' + participants);
            } else if (!hasDatePassed(event.startDate)) {
                handleErrorMessage('', 'errorCodeMessages.messageEventJoinStartedOrFinished');
                $uibModalInstance.close();
            } else if (!hasDatePassed(event.registrationDeadlineDate)) {
                handleErrorMessage('', 'events.eventJoinRegistrationDeadlinePassed');
                $uibModalInstance.close();
            } else {
                if (vm.isAddColleague) {
                    var participantIds = lodash.map(vm.participants, 'id');
                    eventRepository.addColleagues(event.id, vm.selectedOptions, participantIds)
                        .then(handleSuccessPromise, handleErrorPromise);
                } else {
                    eventRepository.joinEvent(event.id, vm.selectedOptions)
                        .then(handleSuccessPromise, handleErrorPromise);
                }
            }
        }

        function handleSuccessPromise() {
            if (isDetails || vm.isAddColleague) {
                eventRepository.getEventDetails(event.id).then(function(response) {
                    angular.copy(response, event);

                    event.options = response.options;
                    event.participants = response.participants;
                });
            } else {
                event.participantsCount++;
            }

            vm.isActionDisabled = false;
            event.isParticipating = true;
            $uibModalInstance.close();

            notifySrv.success('events.joinedEvent');
        }

        function handleErrorPromise(error) {
            vm.isActionDisabled = false;
            errorHandler.handleErrorMessage(error);
        }

        function handleErrorMessage(message) {
            notifySrv.error(message);
            vm.isActionDisabled = false;
        }

        function closeModal() {
            $uibModalInstance.close();
        }

        function canJoinEvent() {
            if (!hasDatePassed(event.startDate)) {
                notifySrv.error('errorCodeMessages.messageEventJoinStartedOrFinished');
                return false;
            } else if (!hasDatePassed(event.registrationDeadlineDate)) {
                notifySrv.error('events.eventJoinRegistrationDeadlinePassed');
                return false;
            }

            return true;
        }

        function isOptionsJoinAvailable() {
            var selectedOptionsCount = vm.selectedOptions.length;

            return !!event.maxChoices && (!selectedOptionsCount || selectedOptionsCount > event.maxChoices);
        }

        function isTooManyOptionsSelected() {
            return vm.selectedOptions.length > event.maxChoices;
        }

        function hasDatePassed(date) {
            return moment.utc(date).local().isAfter();
        }
    }
})();
