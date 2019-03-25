describe('aceEventsListTabs', function() {
    var element, $q, scope, ctrl;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('eventRepository', eventsMocks.eventRepository);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });

    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.eventRepository, 'getEventTypes').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.tabs);
            return deferred.promise;
        });
    }));

    describe('eventsListTabs', function() {
        it('should be initialized', function() {
            inject(function($compile, $rootScope, $templateCache) {
                scope = $rootScope.$new();

                $templateCache.put('app/events/list/list-tabs/list-tabs.html', '<div></div>');
                element = angular.element('<ace-events-list-tabs></ace-events-list-tabs>');

                $compile(element)(scope);
                scope.$digest();
                ctrl = element.controller('aceEventsListTabs');
            })
            expect(ctrl).toBeDefined();

            expect(ctrl.eventsTabs).toBeDefined();
        });

/*        it('should add default tabs to eventsTabs array in correct order', function() {
            setTimeout(function() {
                expect(ctrl.eventsTabs[0]).toEqual(eventsMocks.allEventsTab);
                expect(ctrl.eventsTabs[4]).toEqual(eventsMocks.participatedTab);
                expect(ctrl.eventsTabs[5]).toEqual(eventsMocks.hostTab);
                expect(ctrl.eventsTabs.length).toEqual(6);
            }, 100);
        });

        it('should contain returned tabs inside eventsTabs array', function() {
            expect(ctrl.eventsTabs[1]).toEqual(eventsMocks.tabs[0]);
            expect(ctrl.eventsTabs[2]).toEqual(eventsMocks.tabs[1]);
            expect(ctrl.eventsTabs[3]).toEqual(eventsMocks.tabs[2]);
        });*/

    });
});
