describe('aceEventDescription', function () {
    var element, scope, ctrl, $q;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('$state', eventsMocks.$state);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('eventRepository', eventsMocks.eventRepository);
            $provide.value('notifySrv', eventsMocks.notifySrv);
            $provide.value('errorHandler', eventsMocks.errorHandler);
        });
    });

    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.notifySrv, 'success').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        spyOn(eventsMocks.notifySrv, 'error').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
    }));

    beforeEach(inject(function ($compile, $rootScope, $templateCache) {
        scope = $rootScope.$new();

        scope.isDetails = false;
        scope.event = eventsMocks.events[0];

        $templateCache.put('app/events/content/description/description.html', '<div></div>');
        element = angular.element('<ace-event-description event="event"></ace-event-description>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceEventDescription');
    }));
});
