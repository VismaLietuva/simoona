describe('EventContentController', function () {
    var $rootScope, scope, ctrl, $q;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('errorHandler', eventsMocks.errorHandler);
            $provide.value('$location', eventsMocks.$location);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });
    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.eventRepository, 'getEventDetails').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.sampleEvent);
            return deferred.promise;
        });
        spyOn(eventsMocks.notifySrv, 'success').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.errorHandler, 'handleErrorMessage').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.$state, 'go').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
        spyOn(eventsMocks.$translate, 'instant').and.callFake(function (key) {
            return key;
        });
    }));
    beforeEach(inject(function (_$controller_,  _$rootScope_) {
        $rootScope = _$rootScope_;
        scope = $rootScope.$new();

        ctrl = _$controller_('EventContentController', {
            $rootScope: $rootScope,
            $stateParams: eventsMocks.$stateParams,
            $state: eventsMocks.$state,
            eventRepository: eventsMocks.eventRepository,
            authService: eventsMocks.authService,
            notifySrv: eventsMocks.notifySrv
        });

        $rootScope.$digest();
    }));

    it('should be initialized', function () {
        expect(ctrl).toBeDefined();
    });

    describe('getEvent', function () {
        it('should set event, permissions and participant emails',function() {
            ctrl.getEvent();

            expect(ctrl.event).toEqual(eventsMocks.sampleEvent);
            expect(ctrl.event.participantsCount).toBeDefined();
            expect(ctrl.isEventAdmin).toBeDefined();
        });

        it('should show error message and redirect to events list', function () {
            eventsMocks.eventRepository.getEventDetails.and.callFake(function() {
                var deferred = $q.defer();
                deferred.reject({data: {
                    message: 'Error happend'
                }});
                return deferred.promise;
            });

            ctrl.getEvent();
            $rootScope.$digest();

            expect(eventsMocks.errorHandler.handleErrorMessage).toHaveBeenCalled();

            expect(eventsMocks.$state.go).toHaveBeenCalled();
            expect(eventsMocks.$state.go).toHaveBeenCalledWith('Root.WithOrg.Client.Events.List.Type', {type: 'all'});
        });
    });
});
