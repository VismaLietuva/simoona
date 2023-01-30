(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .constant('eventSettings', {
            nameLength: 35,
            locationLength: 50,
            participantsLength: 1000,
            descriptionLength: 5000,
            thumbHeight: 165,
            thumbWidth: 291,
            endDateHoursAddition: 2,
            minOptions: 2,
        })
        .constant('recurringTypesResources', {
            0: 'none',
            1: 'everyDay',
            2: 'everyWeek',
            3: 'everyTwoWeeks',
            4: 'everyMonth',
        })
        .constant('reminderTypes', {
            start: 0,
            deadline: 1
        })
        .constant('reminderDefaultValues', {
            remindBeforeDays: 7,
            maxRemindBeforeDays: 100
        })
        .controller('addNewEventController', addNewEventController);

    addNewEventController.$inject = [
        '$rootScope',
        '$scope',
        '$stateParams',
        '$state',
        '$timeout',
        'dataHandler',
        'authService',
        'eventRepository',
        'pictureRepository',
        'eventSettings',
        'recurringTypesResources',
        'notifySrv',
        'localeSrv',
        'lodash',
        'errorHandler',
        'optionRules',
        'reminderTypes',
        'reminderDefaultValues'
    ];

    function addNewEventController(
        $rootScope,
        $scope,
        $stateParams,
        $state,
        $timeout,
        dataHandler,
        authService,
        eventRepository,
        pictureRepository,
        eventSettings,
        recurringTypesResources,
        notifySrv,
        localeSrv,
        lodash,
        errorHandler,
        optionRules,
        reminderTypes,
        reminderDefaultValues
    ) {
        /* jshint validthis: true */
        var vm = this;

        vm.maxRemindBeforeDays = reminderDefaultValues.maxRemindBeforeDays;
        vm.reminders = createReminders();

        vm.states = {
            isAdd: $state.includes('Root.WithOrg.Client.Events.AddEvents'),
            isEdit: $state.includes('Root.WithOrg.Client.Events.EditEvent'),
        };

        vm.resetParticipantList = false;
        vm.resetVirtualParticipantList = false;
        vm.isRegistrationDeadlineEnabled = false;
        vm.isSaveButtonEnabled = true;
        vm.allowJoiningVirtually = false;
        vm.eventSettings = eventSettings;
        vm.eventImageSize = {
            w: eventSettings.thumbWidth,
            h: eventSettings.thumbHeight,
        };
        vm.endDateHoursAddition = eventSettings.endDateHoursAddition;
        vm.recurringTypesResources = recurringTypesResources;

        $rootScope.pageTitle = vm.states.isAdd
            ? 'events.addTitle'
            : 'events.editTitle';

        vm.isOfficeSelected = null;
        vm.eventOffices = [];
        vm.eventTypes = [];
        vm.event = {};
        vm.event.options = [];
        vm.eventImage = '';
        vm.eventCroppedImage = '';
        vm.minStartDate = moment().local().startOf('minute').toDate();

        vm.toggleOfficeSelection = toggleOfficeSelection;
        vm.toggleAllOffices = toggleAllOffices;
        vm.searchUsers = searchUsers;
        vm.addOption = addOption;
        vm.deleteOption = deleteOption;
        vm.countOptions = countOptions;
        vm.isValidOption = isValidOption;
        vm.isOptionsUnique = isOptionsUnique;
        vm.togglePin = togglePin;
        vm.saveEvent = saveEvent;
        vm.deleteEvent = deleteEvent;
        vm.createEvent = createEvent;
        vm.updateEvent = updateEvent;
        vm.openDatePicker = openDatePicker;
        vm.showRegistrationDeadline = showRegistrationDeadline;
        vm.getResponsiblePerson = getResponsiblePerson;
        vm.setEvent = setEvent;
        vm.closeAllDatePickers = closeAllDatePickers;
        vm.isStartDateValid = isStartDateValid;
        vm.isEndDateValid = isEndDateValid;
        vm.isDeadlineDateValid = isDeadlineDateValid;
        vm.isOneTimeEvent = isOneTimeEvent;
        vm.resetReminder = resetReminder;
        vm.isReminderDisabled = isReminderDisabled;

        init();

        ///////

        function init() {
            vm.isOptions = false;
            vm.isIgnoreSingleJoinEnabled = false;

            vm.datePickers = {
                isOpenEventStartDatePicker: false,
                isOpenEventFinishDatePicker: false,
                isOpenEventDeadlineDatePicker: false,
            };

            eventRepository.getEventOffices().then(function (response) {
                vm.eventOffices = response;
            });

            eventRepository.getEventTypes().then(function (response) {
                vm.eventTypes = response;
            });

            eventRepository.getEventRecurringTypes().then(function (response) {
                vm.recurringTypes = response;
            });

            function setEventTypes() {
                $scope.$watch(
                    function () {
                        return vm.eventTypes;
                    },
                    function () {
                        if (vm.eventTypes.length) {
                            vm.eventTypes.forEach(function (type) {
                                if (type.id == vm.event.typeId) {
                                    vm.selectedType = type;
                                }
                            });
                        }
                    }
                );
            }

            if ($stateParams.id) {
                eventRepository.getEventUpdate($stateParams.id).then(
                    function (event) {
                        vm.event = event;
                        vm.allowJoiningVirtually =
                            event.maxVirtualParticipants > 0;
                        setEventTypes();
                        vm.responsibleUser = {
                            id: vm.event.hostUserId,
                            fullName: vm.event.hostUserFullName,
                        };

                        vm.minParticipants = vm.event.maxParticipants;
                        vm.minVirtualParticipants = vm.event.maxVirtualParticipants;

                        if (
                            vm.event.startDate !==
                            vm.event.registrationDeadlineDate
                        ) {
                            vm.isRegistrationDeadlineEnabled = true;
                            vm.reminders[reminderTypes.deadline].isVisible = true;
                        }
                        vm.event.offices = [];
                        vm.event.officeIds.forEach(function (value) {
                            vm.event.offices.push(value);
                        });
                        vm.event.registrationDeadlineDate = moment
                            .utc(vm.event.registrationDeadlineDate)
                            .local()
                            .startOf('minute')
                            .toDate();

                        vm.event.startDate = moment
                            .utc(vm.event.startDate)
                            .local()
                            .startOf('minute')
                            .toDate();

                        vm.event.endDate = moment
                            .utc(vm.event.endDate)
                            .local()
                            .startOf('minute')
                            .toDate();

                        if (!!vm.event.options.length) {
                            vm.isOptions = true;
                            setIgnoreSingleJoinOption(vm.event.options);
                        } else {
                            vm.event.maxOptions = 1;
                            addOption();
                        }

                        validateOfficeSelection();
                        updateReminders();
                    },
                    function (error) {
                        errorHandler.handleErrorMessage(error);

                        $state.go('Root.WithOrg.Client.Events.List.Type', {
                            type: 'all',
                        });
                    }
                );
            } else {
                getResponsiblePerson(authService.identity.userId);

                vm.event = {
                    name: '',
                    typeId: null,
                    offices: [],
                    startDate: moment()
                        .add(1, 'hours')
                        .local()
                        .startOf('minute')
                        .toDate(),
                    endDate: moment()
                        .add(3, 'hours')
                        .local()
                        .startOf('minute')
                        .toDate(),
                    isPinned: false,
                    allowMaybeGoing: true,
                    allowNotGoing: true,
                    recurrence: 0,
                    location: '',
                    description: '',
                    imageName: '',
                    maxOptions: 1,
                    options: [],
                    registrationDeadlineDate: null,
                    maxVirtualParticipants: 0,
                };

                eventRepository
                    .getMaxEventParticipants()
                    .query(function (response) {
                        vm.event.maxParticipants = response.value;
                    });

                addOption();
            }

            $scope.$watch(
                'vm.responsibleUser',
                function (newVal) {
                    if (newVal && !newVal.id) {
                        vm.isResponsibleUserError = true;
                    } else {
                        vm.isResponsibleUserError = null;
                    }
                },
                true
            );
        }

        function updateReminders() {
            for (var reminder of vm.event.reminders) {
                var localReminder = vm.reminders[reminder.type];
                localReminder.isVisible = true;
                localReminder.isEnabled = true;
                localReminder.value = reminder.remindBeforeInDays;
                localReminder.isDisabled = reminder.isDisabled;
                localReminder.isCreated = true;
            }
        }

        function toggleOfficeSelection(office) {
            var idx = vm.event.offices.indexOf(office.id);

            if (idx > -1) {
                vm.event.offices.splice(idx, 1);
            } else {
                vm.event.offices.push(office.id);
            }

            validateOfficeSelection();
        }

        function toggleAllOffices() {
            if (vm.event.offices.length == vm.eventOffices.length) {
                vm.event.offices = [];
            } else {
                vm.event.offices = [];
                angular.forEach(vm.eventOffices, function (office) {
                    vm.event.offices.push(office.id);
                });
            }

            validateOfficeSelection();
        }

        function validateOfficeSelection() {
            if (vm.event.offices.length < 1) {
                vm.isOfficeSelected = false;
            } else {
                vm.isOfficeSelected = true;
            }
        }

        function searchUsers(search) {
            return eventRepository.getUserForAutoCompleteResponsiblePerson(
                search
            );
        }

        function getResponsiblePerson(userId) {
            eventRepository
                .getUserResponsiblePersonById(userId)
                .then(function (data) {
                    vm.responsibleUser = data;
                });
        }

        function isValidOption(options, option) {
            for (var i = 0; options.length > i; i++) {
                if (
                    options.indexOf(option) !== i &&
                    option.option &&
                    options[i].option &&
                    option.option === options[i].option
                ) {
                    return false;
                }
            }

            return true;
        }

        function isOptionsUnique() {
            if (!!vm.isOptions) {
                var tempArray = angular.copy(vm.event.options);
                var uniqueOptions = lodash.uniq(tempArray, 'option');
                return lodash.isEqual(tempArray.sort(), uniqueOptions.sort());
            } else {
                return true;
            }
        }

        function addOption() {
            vm.event.options.push({
                option: '',
            });
        }

        function deleteOption(index) {
            vm.event.options.splice(index, 1);
            handleOptions();
        }

        function resetReminder(type) {
            vm.reminders[type].value = reminderDefaultValues.remindBeforeDays;
        }

        function handleOptions() {
            var optionsSum = countOptions();

            if (optionsSum < eventSettings.minOptions) {
                vm.isOptions = false;
                vm.isIgnoreSingleJoinEnabled = false;
                addOption();
            }
        }

        function countOptions() {
            return vm.isIgnoreSingleJoinEnabled &&
                vm.selectedType &&
                vm.selectedType.isSingleJoin
                ? vm.event.options.length + 1
                : vm.event.options.length;
        }

        function togglePin() {
            vm.event.isPinned = !vm.event.isPinned;
        }

        function saveEvent(method, newImage) {
            if (newImage.length) {
                var eventImageBlob = dataHandler.dataURItoBlob(
                    vm.eventCroppedImage[0],
                    vm.eventImage[0].type
                );

                eventImageBlob.lastModifiedDate = new Date();
                eventImageBlob.name = vm.eventImage[0].name;

                pictureRepository
                    .upload([eventImageBlob])
                    .then(function (result) {
                        method(result.data);
                    });
            } else {
                method();
            }
        }

        function deleteEvent(id) {
            eventRepository.deleteEvent(id).then(function (result) {
                notifySrv.success('events.successDelete');

                $state.go('Root.WithOrg.Client.Events.List.Type', {
                    type: 'all',
                });
            }, errorHandler.handleErrorMessage);
        }

        function createEvent(image) {
            if (vm.isSaveButtonEnabled) {
                vm.isSaveButtonEnabled = false;

                setEvent();

                if (image) {
                    vm.event.imageName = image;
                }

                eventRepository.createEvent(vm.event).then(
                    function (result) {
                        notifySrv.success('common.successfullySaved');

                        $state.go('Root.WithOrg.Client.Events.EventContent', {
                            id: result.id,
                        });
                    },
                    function (error) {
                        vm.isSaveButtonEnabled = true;
                        errorHandler.handleErrorMessage(error);
                    }
                );
            }
        }

        function createReminders() {
            return {
                [reminderTypes.start]: {
                    isVisible: true,
                    isEnabled: false,
                    value: reminderDefaultValues.remindBeforeDays,
                    translation: 'events.remindDaysBeforeEventStart',
                    isDisabled: false
                },
                [reminderTypes.deadline]: {
                    isVisible: false,
                    isEnabled: false,
                    value: reminderDefaultValues.remindBeforeDays,
                    translation: 'events.remindDaysBeforeEventDeadline',
                    isDisabled: false
                }
            }
        }

        function isReminderDisabled(reminderType) {
            if (vm.reminders[reminderType].isDisabled) {
                return true;
            }

            return !canReminderBeModified(reminderType);
        }

        function canReminderBeModified(reminderType) {
            var eventDate = getDateFromEventBasedOnReminderType(reminderType);
            var currentDate = moment()
                .local()
                .startOf('minute')
                .toDate();
            return eventDate > currentDate;
        }

        function getDateFromEventBasedOnReminderType(reminderType) {
            reminderType = parseInt(reminderType);
            switch (reminderType) {
                case reminderTypes.start:
                    return vm.event.startDate;
                case reminderTypes.deadline:
                    return vm.event.registrationDeadlineDate ?? vm.event.startDate;
                default:
                    console.error('Reminder type ' + reminderType + ' is not supported');
            }
        }

        function updateEvent(image) {
            if (vm.isSaveButtonEnabled) {
                vm.isSaveButtonEnabled = false;

                setEvent();

                if (image) {
                    vm.event.imageName = image;
                }

                eventRepository.updateEvent(vm.event).then(
                    function (result) {
                        notifySrv.success('common.successfullySaved');

                        $state.go('Root.WithOrg.Client.Events.EventContent', {
                            id: result.id,
                        });
                    },
                    function (error) {
                        vm.isSaveButtonEnabled = true;
                        errorHandler.handleErrorMessage(error);
                    }
                );
            }
        }

        function setEvent() {
            manageParticipantResets();

            if (!vm.isRegistrationDeadlineEnabled) {
                vm.event.registrationDeadlineDate = vm.event.startDate;
            }

            if (vm.isOptions) {
                var tempArray = [];
                vm.event.editedOptions = [];
                vm.event.newOptions = [];

                tempArray = lodash.filter(vm.event.options, function (element) {
                    return !element.id;
                });

                vm.event.newOptions = lodash.map(tempArray, (obj) => {
                    return {
                        option: obj.option,
                        rule: optionRules.default,
                    };
                });

                vm.event.editedOptions = lodash.filter(
                    vm.event.options,
                    function (element) {
                        return !!element.id;
                    }
                );

                if (
                    vm.isIgnoreSingleJoinEnabled &&
                    vm.selectedType.isSingleJoin
                ) {
                    var optionValue;

                    if (
                        !vm.ignoreSingleJoinOption ||
                        !vm.ignoreSingleJoinOption.option
                    ) {
                        optionValue = localeSrv.translate(
                            'events.ignoreSingleJoinDefaultOption'
                        );
                    } else {
                        optionValue = vm.ignoreSingleJoinOption.option;
                    }

                    if (
                        vm.ignoreSingleJoinOption &&
                        vm.ignoreSingleJoinOption.id
                    ) {
                        vm.event.editedOptions.push({
                            id: vm.ignoreSingleJoinOption.id,
                            option: optionValue,
                            rule: optionRules.default,
                        });
                    } else {
                        vm.event.newOptions.push({
                            option: optionValue,
                            rule: optionRules.ignoreSingleJoin,
                        });
                    }
                }
            } else {
                vm.event.options = [];
                vm.event.editedOptions = [];
                vm.event.newOptions = [];

                vm.event.maxOptions = 0;
            }

            vm.event.responsibleUserId = vm.responsibleUser.id;

            vm.event.endDate = moment(vm.event.endDate)
                .local()
                .startOf('minute')
                .toDate();

            vm.event.reminders = mapRemindersToRequestParameters();
        }

        function mapRemindersToRequestParameters() {
            if (!isOneTimeEvent()) {
                return [];
            }

            return Object.keys(vm.reminders)
                .filter(key => (vm.reminders[key].isEnabled &&
                                vm.reminders[key].isVisible &&
                                (canReminderBeModified(key) ||
                                vm.reminders[key].isCreated)) ||
                                vm.reminders[key].isDisabled)
                .map(key => ({ remindBeforeInDays:  vm.reminders[key].value, type: key }));
        }

        function isOneTimeEvent() {
            return vm.recurringTypesResources[vm.event.recurrence] === 'none';
        }

        function manageParticipantResets() {
            if (vm.states.isEdit && vm.resetParticipantList) {
                vm.event.resetParticipantList =
                    vm.event.maxParticipants < vm.minParticipants;
            }

            if (!vm.allowJoiningVirtually && vm.minVirtualParticipants) {
                vm.event.maxVirtualParticipants = 0;
                vm.event.resetVirtualParticipantList = true;
                return;
            }

            if (vm.states.isEdit && vm.resetVirtualParticipantList) {
                vm.event.resetVirtualParticipantList =
                    vm.event.maxVirtualParticipants < vm.minVirtualParticipants;
            }
        }

        function showRegistrationDeadline() {
            if (vm.isRegistrationDeadlineEnabled) {
                vm.event.registrationDeadlineDate = vm.event.startDate;
            }

            var deadlineReminder = vm.reminders[reminderTypes.deadline];
            if (deadlineReminder.isDisabled) {
                return;
            }
            deadlineReminder.isVisible = !deadlineReminder.isVisible;
        }

        function openDatePicker($event, datePicker) {
            $event.preventDefault();
            $event.stopPropagation();

            vm.closeAllDatePickers(datePicker);

            $timeout(function () {
                $event.target.focus();
            }, 100);
        }

        function closeAllDatePickers(datePicker) {
            vm.datePickers.isOpenEventStartDatePicker = false;
            vm.datePickers.isOpenEventDeadlineDatePicker = false;
            vm.datePickers.isOpenEventFinishDatePicker = false;

            vm.datePickers[datePicker] = true;
        }

        function isStartDateValid() {
            if (vm.states.isAdd && vm.event.startDate) {
                return vm.minStartDate < vm.event.startDate;
            }

            return true;
        }

        function isEndDateValid() {
            if (vm.event.endDate) {
                return vm.event.endDate > vm.event.startDate;
            }

            return true;
        }

        function isDeadlineDateValid() {
            if (vm.states.isAdd) {
                return (
                    vm.isRegistrationDeadlineEnabled &&
                    (vm.event.registrationDeadlineDate > vm.event.startDate ||
                        vm.event.registrationDeadlineDate < vm.minStartDate)
                );
            } else {
                return (
                    vm.isRegistrationDeadlineEnabled &&
                    (vm.event.registrationDeadlineDate > vm.event.startDate ||
                        !vm.event.registrationDeadlineDate)
                );
            }
        }

        function setIgnoreSingleJoinOption(options) {
            var index = options.findIndex(
                (o) => o.rule == optionRules.ignoreSingleJoin
            );

            if (index > -1) {
                vm.isIgnoreSingleJoinEnabled = true;
                vm.ignoreSingleJoinOption = options[index];

                vm.event.options.splice(index, 1);
            }
        }
    }
})();
