describe('aceEventParticipant', function () {
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

        scope.participant = eventsMocks.events[0].participants[0];

        $templateCache.put('app/events/content/participants/participant-list/participant/participant.html', '<div></div>');
        element = angular.element('<ace-event-participant participant="participant"></ace-event-participant>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceEventParticipant');
    }));

    it('it should be defined', function () {
        expect(ctrl).toBeDefined();
    });
});
