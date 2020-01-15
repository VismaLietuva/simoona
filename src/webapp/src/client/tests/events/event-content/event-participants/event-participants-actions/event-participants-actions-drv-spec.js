describe('aceEventParticipantsActions', function () {
    var element, scope, ctrl, $q;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('notifySrv', eventsMocks.notifySrv);
            $provide.value('errorHandler', eventsMocks.errorHandler);
            $provide.value('eventRepository', eventsMocks.eventRepository);
            $provide.value('Analytics', eventsMocks.Analytics);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });

    beforeEach(inject(function(_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.eventRepository, 'resetParticipantList').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
    }));

    beforeEach(inject(function ($compile, $rootScope, $templateCache) {
        scope = $rootScope.$new();

        scope.isEventAdmin = false;
        scope.event = eventsMocks.sampleEvent;
        scope.options = eventsMocks.sampleEvent.options;

        $templateCache.put('app/events/content/participants/participants-actions/participants-actions.html', '<div></div>');
        element = angular.element('<ace-event-participants-actions event="event" is-admin="isEventAdmin" options="options"></ace-event-participants-actions>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceEventParticipantsActions');
    }));

    it('should reset participant list on method call resetParticipantList', function () {
        ctrl.event.participatingStatus = 1;
        ctrl.resetParticipantList();
        scope.$digest();

        expect(eventsMocks.eventRepository.resetParticipantList).toHaveBeenCalled();
        expect(eventsMocks.eventRepository.resetParticipantList).toHaveBeenCalledWith(ctrl.event.id);

        expect(ctrl.event.participants.length).toBeFalsy();
        expect(ctrl.event.participatingStatus).toBeFalsy();
    });

});
