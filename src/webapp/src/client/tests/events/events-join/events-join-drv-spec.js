describe('aceEventJoin', function() {
    var element, scope, ctrl, $q;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('$state', eventsMocks.$state);
            $provide.value('$uibModal', eventsMocks.$uibModal);
            $provide.value('notifySrv', eventsMocks.notifySrv);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('eventRepository', eventsMocks.eventRepository);
            $provide.value('eventsMessages', eventsMocks.eventsMessages);
            $provide.value('localeSrv', eventsMocks.localeSrv);
            $provide.value('errorHandler', eventsMocks.errorHandler);
            $provide.value('Analytics', eventsMocks.Analytics);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });

    beforeEach(inject(function(_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.eventRepository, 'getEventOptions').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.events[0]);
            return deferred.promise;
        });

        spyOn(eventsMocks.eventRepository, 'getEventUpdate').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.events[0]);
            return deferred.promise;
        });

        spyOn(eventsMocks.notifySrv, 'success').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        spyOn(eventsMocks.errorHandler, 'handleErrorMessage').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        spyOn(eventsMocks.errorHandler, 'handleError').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        spyOn(eventsMocks.notifySrv, 'error').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        
        spyOn(eventsMocks.$uibModal, 'open').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
    }));

    describe('joinEvent', function() {
        beforeEach(inject(function($compile, $rootScope, $templateCache) {
            scope = $rootScope.$new();

            scope.isDetails = false;
            scope.isAddColleague = true;
            scope.event = eventsMocks.events[0];

            $templateCache.put('app/events/join/join.html', '<div></div>');
            element = angular.element('<ace-event-join event="event" is-details="isDetails" is-add-colleague="isAddColleague"></ace-event-join>');

            $compile(element)(scope);
            scope.$digest();

            ctrl = element.controller('aceEventJoin');
        }));

        it('user should see join modal popup if isAddColleague is true', function() {
            spyOn(eventsMocks.eventRepository, 'joinEvent').and.callFake(function() {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });

            ctrl.joinEvent();
            scope.$digest();

            expect(eventsMocks.$uibModal.open).toHaveBeenCalled();
        });
    });

    describe('leaveEvent', function() {
        beforeEach(inject(function($compile, $rootScope, $templateCache) {
            scope = $rootScope.$new();

            scope.isDetails = false;
            scope.isAddColleague = false;
            scope.event = eventsMocks.events[0];

            $templateCache.put('app/events/join/join.html', '<div></div>');
            element = angular.element('<ace-event-join event="event" is-details="isDetails" is-add-colleague="isAddColleague"></ace-event-join>');

            $compile(element)(scope);
            scope.$digest();

            ctrl = element.controller('aceEventJoin');
        }));

        it('user should be able to leave event if AddColleague is falsy', function() {
            spyOn(eventsMocks.eventRepository, 'leaveEvent').and.callFake(function() {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });

            var oldParticipantsCount = scope.event.participantsCount;
            ctrl.leaveEvent(scope.event.id);
            scope.$digest();

            expect(eventsMocks.eventRepository.leaveEvent).toHaveBeenCalled();
            expect(scope.event.isParticipating).toEqual(false);
            expect(scope.event.participantsCount).toEqual(oldParticipantsCount - 1);
        });

        it('user should see error message if leaveEvent responds error', function() {
            spyOn(eventsMocks.eventRepository, 'leaveEvent').and.callFake(function() {
                var deferred = $q.defer();
                deferred.reject();
                return deferred.promise;
            });

            ctrl.leaveEvent(scope.event.id);
            scope.$digest();

            expect(eventsMocks.errorHandler.handleError).toHaveBeenCalled();
        });
    });
});
