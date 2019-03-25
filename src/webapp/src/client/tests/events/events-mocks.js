var eventsMocks = {};

beforeEach(function () {
    eventsMocks = {
        $windowProvider: {
            $get: function() {
                return {
                    isPremium: false
                }
            }
        },
        $resource: {},
        endPoint: '',
        participantInput: 'image.jpg',
        participantThumb: {
            width: 100,
            height: 100,
            mode: 'crop'
        },
        $location: {
            hash: function() {}
        },
        posts: [
            {id: 1, messageBody: 'abc', pictureId: 'abc.jpg'},
            {id: 2, messageBody: 'def', pictureId: 'def.jpg'}
        ],
        beforeOneHour: moment().subtract(1, 'hours').local(),
        nowDate: moment(new Date()).local(),
        afterOneHour: moment().add(1, 'hours').local(),
        afterTwoHours: moment().add(2, 'hours').local(),
        afterThreeHours: moment().add(3, 'hours').local(),
        inProgressEvent: {
            startDate: eventsMocks.beforeOneHour,
            endDate: eventsMocks.afterOneHour
        },
        finishedEvent: {
            startDate: eventsMocks.beforeOneHour,
            endDate: eventsMocks.nowDate
        },
        registrationIsClosedEvent: {
            registrationDeadlineDate: eventsMocks.beforeOneHour,
            startDate: eventsMocks.afterOneHour,
            endDate: eventsMocks.afterTwoHours
        },
        fullEvent: {
            maxParticipants : 10,
            participantsCount: 10,
            isParticipating: false,
            registrationDeadlineDate: eventsMocks.afterOneHour,
            startDate: eventsMocks.afterTwoHours,
            endDate: eventsMocks.afterThreeHours
        },
        joinEvent: {
            maxParticipants : 10,
            participantsCount: 0,
            isParticipating: false,
            registrationDeadlineDate: eventsMocks.afterOneHour,
            startDate: eventsMocks.afterTwoHours,
            endDate: eventsMocks.afterThreeHours
        },
        eventStatusService: {
            getEventStatus: function () {}
        },
        eventStatus: {
            InProgress : 1,
            Finished : 2,
            RegistrationIsClosed : 3,
            Full : 4,
            Join : 5
        },
        errorHandler: {
            handleErrorMessage: function () {},
            handleError: function () {}
        },
        localeSrv: {
            formatTranslation: function(key) { return key; }
        },
        $uibModal: {
            open: function () {}
        },
        $state: {
            params: {
                type: ''
            },
            includes : function() {},
            go: function() {}
        },
        $timeout: function() {},
        $event: {
            preventDefault: function() {},
            stopPropagation: function() {}
        },
        $translate: {
            instant: function () {}
        },
        notifySrv: {
            error: function () {},
            success: function () {}
        },
        uniqueOptions: [
            {id: 1, option: 'a'}, {id: 2, option: 'b'}, {id: 3, option: 'c'}
        ],
        notUniqueOptions: [
            {id: 1, option: 'b'}, {id: 2, option: 'b'}
        ],
        responsibleUserResponse: {
            id: 1,
            fullName: 'John Johnson'
        },
        sampleEvent: {
            id: 1,
            name: '5a picos',
            place: 'Vilnius',
            eventTypeId: 1,
            imageName: 'Nuotrauka.png',
            recurring: 1,
            description: 'description',
            maxParticipants: 10,
            maxChoices: 2,
            participants: [
                {id: 1, userId: 1, fullName: 'First User'},
                {id: 2, userId: 2, fullName: 'Second User'},
                {id: 3, userId: 3, fullName: 'Third User'}
            ],
            options: [
                {id: 1, option: 'a', participants: [{userId: 1}, {userId: 2}]},
                {id: 2, option: 'b', participants: [{userId: 3}]},
                {id: 3, option: 'c', participants: []}
            ],
            startDate: '2016-01-00T00:00:00.000',
            responsibleUser: {id: 1}
        },
        participantList: [{id: 1, userId: 1, fullName: 'First User'}, {id: 3, userId: 3, fullName: 'Third User'}],
        optionsList: [{id: 1, option: 'a', participants: [{userId: 1}, {userId: 2}]}, {id: 2, option: 'b', participants: []}, {id: 3, option: 'c', participants: []}],
        participantsWithoutFirstUser: [
                {
                    fullName: 'Second User',
                    userId: 2,
                    id: 2
                },
                {
                    fullName: 'Third User',
                    userId: 3,
                    id: 3
                }
        ],
        events: [
            {id: 1, typeId: 1, name: '5a Picos', startDate: '2999-05-06T09:34:15.147', registrationDeadlineDate: '2998-05-06T09:34:15.147', isParticipating: false, participantsCount: 5, responsibleUser: {id: 1}, maxChoices: 2,
                participants: [{}], options: [{id: 1, option: 'a'}, {id: 2, option: 'b'}, {id: 3, option: 'c'}]},
            {id: 2, typeId: 1, name: '6a Picos', options: [{name: 'First option'}, {name: 'Second option'}]},
            {id: 3, typeId: 1, name: '6a Chikens', options: []},
            {id: 4, typeId: 2, name: 'Begimas miske', options: []},
            {id: 5, typeId: 3, name: 'Board games', options: []}
        ],
        eventTypes: [
            {id: 1, name: 'foodEvent'},
            {id: 2, name: 'sportEvent'},
            {id: 3, name: 'leisureEvent'}
        ],
        eventTypesExpectedAfterInit: [
            {id: 'All', name: 'allEvents'},
            {id: 'MyEvents', name: 'myEvents'}
        ],
        getEventsByType: function(typeId) {
            if (typeId) {
                var result = [];
                for (var i = 0; eventsMocks.events.length > i; i++) {
                    if (eventsMocks.events[i].typeId === typeId) {
                        result.push(eventsMocks.events[i]);
                    }
                }

                return result;
            } else {
                return eventsMocks.events;
            }
        },
        eventRepository: {
            getEventDetails: function() {},
            createEvent: function () {},
            updateEvent: function () {},
            getEventTypes: function () {},
            getAllEvents: function () {},
            getEventsByType: function () {},
            getMyEvents: function () {},
            deleteEvent: function () {},
            getEventUpdate: function () {},
            joinEvent: function () {},
            listJoin: function () {},
            leaveEvent: function () {},
            getEventOptions: function () {},
            getEventRecurringTypes: function () {},
            getResponsiblePerson: function() {},
            searchUsers: function() {},
            getUserResponsiblePersonById: function () {},
            getUserForAutoCompleteResponsiblePerson: function () {},
            resetParticipantList: function () {},
            expelUserFromEvent: function () {},
            getMaxEventParticipants: function() {}
        },
        pictureRepository: {},
        eventsMessages: {
            200: 'invalidType',
            201: ['errorMessage', 'invalidType']
        },
        eventSettings: {},
        recurringTypesResources: {},
        shroomsFileUploader: {},
        eventImageSettings: {},
        dataHandler: {},
        authService: {
            hasPermissions: function () {
                return true;
            },
            identity: {
                userId: 1,
                organizationName: "test"
            }
        },
        Analytics: {
            trackEvent: function () {}
        },
        $stateParams: {
            id: 1
        },
        $stateParamsAll: {
            type: 'all'
        },
        $stateParamsMyEventsMaster: {
            type: 'host'
        },
        $stateParamsByType: {
            type: '1'
        },
        filter: {
            filter: 'host'
        },
        tabs: [
            {
                name: 'first',
                id: 1
            },
            {
                name: 'second',
                id: 2
            },
            {
                name: 'third',
                id: 3
            }
        ],
        allEventsTab: {
            id: 'all',
            name: 'events.all'
        },
        hostTab: {
            id: 'host',
            name: 'events.host'
        },
        participatedTab: {
            id: 'participant',
            name: 'events.participant'
        },
        pictureStorage: 'pictures'
    };
});
