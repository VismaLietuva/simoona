describe('aceEventParticipants', function () {
    var element, scope, ctrl, $q;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('eventRepository', eventsMocks.eventRepository);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('notifySrv', eventsMocks.notifySrv);
            $provide.value('errorHandler', eventsMocks.errorHandler);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('Analytics', eventsMocks.Analytics);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });

    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.eventRepository, 'expelUserFromEvent').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
    }));

    beforeEach(inject(function ($compile, $rootScope, $templateCache) {
        scope = $rootScope.$new();

        scope.isEventLoading = false;
        scope.isEventAdmin = false;
        scope.event = eventsMocks.sampleEvent;
        scope.options = eventsMocks.sampleEvent.options;

        $templateCache.put('app/events/content/participants/participants.html', '<div></div>');
        element = angular.element('<ace-event-participants is-loading="isEventLoading" event="event" is-admin="isEventAdmin" options="options"></ace-event-participants>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceEventParticipants');
    }));

    describe('switch tabs', function() {
        it('should go to OptionsList tab and isActiveTab method should return true for OptionsList', function () {
            expect(ctrl.isActiveTab('ParticipantsList')).toBeTruthy();

            ctrl.goToTab('OptionsList');
            expect(ctrl.isActiveTab('ParticipantsList')).toBeFalsy();
            expect(ctrl.isActiveTab('OptionsList')).toBeTruthy();
        });

        it('should go to ParticipantsList tab and isActiveTab method should return true for ParticipantsList', function () {
            ctrl.goToTab('OptionsList');
            expect(ctrl.isActiveTab('OptionsList')).toBeTruthy();
            expect(ctrl.isActiveTab('ParticipantsList')).toBeFalsy();

            ctrl.goToTab('ParticipantsList');
            expect(ctrl.isActiveTab('ParticipantsList')).toBeTruthy();
            expect(ctrl.isActiveTab('OptionsList')).toBeFalsy();
        });
    });

    describe('isDeleteVisible', function() {
        it('should hide delete button if user is not admin', function() {
            var show = ctrl.isDeleteVisible();

            expect(show).toEqual(false);
        });

        it('should hide delete button if user is admin and event is finished', function() {
            ctrl.isAdmin = true;
            ctrl.event.startDate = eventsMocks.finishedEvent.startDate;
            ctrl.event.endDate = eventsMocks.finishedEvent.endDate;
            var show = ctrl.isDeleteVisible();

            expect(show).toEqual(false);
        });

        it('should show delete button if user is admin and event is not finished', function () {
            ctrl.isAdmin = true;
            ctrl.event.startDate = eventsMocks.inProgressEvent.startDate;
            ctrl.event.endDate = eventsMocks.inProgressEvent.endDate;
            var show = ctrl.isDeleteVisible();

            expect(show).toEqual(true);
        });
    });

    describe('expelUserFromEvent', function() {
        it('should throw error', function() {
            eventsMocks.eventRepository.expelUserFromEvent.and.callFake(function() {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            });

            spyOn(eventsMocks.errorHandler, 'handleErrorMessage');
            ctrl.expelUserFromEvent(eventsMocks.sampleEvent.participants[0]);
            scope.$digest();

            expect(eventsMocks.errorHandler.handleErrorMessage).toHaveBeenCalled();
        });

        it('should remove user from event', function() {
            var participant = eventsMocks.sampleEvent.participants[0];
            ctrl.expelUserFromEvent(participant);
            scope.$digest();

            expect(eventsMocks.eventRepository.expelUserFromEvent).toHaveBeenCalled();
            expect(eventsMocks.eventRepository.expelUserFromEvent).toHaveBeenCalledWith(eventsMocks.sampleEvent.id, participant.userId);
        });

        it('should remove currently logged in user from event', function() {
            var participant = eventsMocks.sampleEvent.participants[0];
            expect(participant.userId).toEqual(eventsMocks.authService.identity.userId);

            ctrl.expelUserFromEvent(participant);
            scope.$digest();

            expect(ctrl.event.participatingStatus).toEqual(0);
        });

        it('should remove user from event participant list', function() {
            var participant = eventsMocks.sampleEvent.participants[1];
            var participantsCount = eventsMocks.sampleEvent.participants.length;
            ctrl.expelUserFromEvent(participant);
            scope.$digest();

            expect(ctrl.event.participants).toEqual(eventsMocks.participantList);
        });

        it('should remove all user options from event option participants', function() {
            var participant = eventsMocks.sampleEvent.participants[2];
            var optionsCount = ctrl.event.options.length;
            ctrl.expelUserFromEvent(participant);
            scope.$digest();

            expect(ctrl.event.options).toEqual(eventsMocks.optionsList);
        });
    });
});
