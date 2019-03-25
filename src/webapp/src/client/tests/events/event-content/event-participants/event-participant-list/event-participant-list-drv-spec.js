describe('aceEventParticipantList', function () {
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

        $templateCache.put('app/events/content/participants/participant-list/participant-list.html', '<div></div>');
        element = angular.element('<ace-event-participant-list event="event"></ace-event-participant-list>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceEventParticipantList');
    }));

    it('it should be defined', function () {
        expect(ctrl).toBeDefined();
    });
});
