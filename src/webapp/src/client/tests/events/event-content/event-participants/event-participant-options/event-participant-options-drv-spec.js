describe('aceEventParticipantOptions', function () {
    var element, scope, ctrl;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });

    beforeEach(inject(function ($compile, $rootScope, $templateCache) {
        scope = $rootScope.$new();

        scope.event = eventsMocks.events[0];

        $templateCache.put('app/events/content/participants/participant-options/participant-options.html', '<div></div>');
        element = angular.element('<ace-event-participant-options event="event"></ace-event-participant-options>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceEventParticipantOptions');
    }));

    it('it should be defined', function () {
        expect(ctrl).toBeDefined();
    });

    it('should return true if current user(First User id:1) is in given participant list', function () {
        ctrl.currentUserId = 1;

        expect(ctrl.hasCurrentUserSelectedOption(eventsMocks.sampleEvent.participants)).toBeTruthy();
    });

    it('should return false if current user(First User id:1) is not in given participant list', function () {
        ctrl.currentUserId = 1;

        expect(ctrl.hasCurrentUserSelectedOption(eventsMocks.participantsWithoutFirstUser)).toBeFalsy();
    });
});
