describe('eventsByTypeController', function () {
    var $rootScope, $q, $controller, scope, ctrl;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });
    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(eventsMocks.eventRepository, 'getAllEvents').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.events);
            return deferred.promise;
        });

        spyOn(eventsMocks.eventRepository, 'getMyEvents').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.events);
            return deferred.promise;
        });

        spyOn(eventsMocks.eventRepository, 'getEventsByType').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(eventsMocks.events);
            return deferred.promise;
        });
    }));

    beforeEach(inject(function (_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;
        $controller = _$controller_;
        scope = $rootScope.$new();

        ctrl = _$controller_('eventsByTypeController', {
            $state: eventsMocks.$state,
            $stateParams: eventsMocks.$stateParamsAll,
            eventRepository: eventsMocks.eventRepository,
            authService: eventsMocks.authService,
            notifySrv: eventsMocks.notifySrv,
            amMoment: {}
        });

        scope.$digest();
    }));

    it('should be initialized', function () {
        ctrl.$stateParams = eventsMocks.$stateParamsAll;

        expect(ctrl).toBeDefined();

        expect(eventsMocks.eventRepository.getAllEvents).toHaveBeenCalled();

        expect(ctrl.eventsList).toBeDefined();
    });

    describe('getMyEvents', function () {
        beforeEach(inject(function (_$controller_, _$rootScope_) {
            $rootScope = _$rootScope_;
            $controller = _$controller_;
            scope = $rootScope.$new();

            ctrl = _$controller_('eventsByTypeController', {
                $state: eventsMocks.$state,
                $stateParams: eventsMocks.$stateParamsMyEventsMaster,
                eventRepository: eventsMocks.eventRepository,
                authService: eventsMocks.authService,
                notifySrv: eventsMocks.notifySrv,
                amMoment: {}
            });

            scope.$digest();
        }));

        it('should call getMyEvents method with type MyEvents if state parameter is MyEvents', function () {
            expect(eventsMocks.eventRepository.getMyEvents).toHaveBeenCalled();
            expect(eventsMocks.eventRepository.getMyEvents).toHaveBeenCalledWith(eventsMocks.filter);
        });
    });

    describe('getEventsByType', function () {
        beforeEach(inject(function (_$controller_, _$rootScope_) {
            $rootScope = _$rootScope_;
            $controller = _$controller_;
            scope = $rootScope.$new();

            ctrl = _$controller_('eventsByTypeController', {
                $state: eventsMocks.$state,
                $stateParams: eventsMocks.$stateParamsByType,
                eventRepository: eventsMocks.eventRepository,
                authService: eventsMocks.authService,
                notifySrv: eventsMocks.notifySrv,
                amMoment: {}
            });

            scope.$digest();
        }));

        it('should call getEventsByType method with type id if state parameter type id is not All or MyEvents', function () {
            expect(eventsMocks.eventRepository.getEventsByType).toHaveBeenCalled();
            expect(eventsMocks.eventRepository.getEventsByType).toHaveBeenCalledWith(eventsMocks.$stateParamsByType.type);
        });
    });
});
