describe('eventsListController', function () {
    var $rootScope, scope, ctrl, mockStateProvider;

    beforeEach(angular.mock.module(function ($windowProvider) {
        $windowProvider.$get = eventsMocks.$windowProvider.$get;
    }));

    beforeEach(function () {
        module('ui.router', function ($stateProvider) {
            mockStateProvider = $stateProvider;
            spyOn(mockStateProvider, 'state').and.callThrough();
        });
    });

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });
    beforeEach(inject(function (_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;
        scope = $rootScope.$new();

        ctrl = _$controller_('eventsListController', {
            $rootScope: $rootScope,
            $state: eventsMocks.$state,
            eventRepository: eventsMocks.eventRepository
        });

        $rootScope.$digest();
    }));

    it('should be initialized', function () {
        expect(ctrl).toBeDefined();
    });

    it('should not register premium routes', function () {
        let state = mockStateProvider.state.calls.allArgs().find(s => s[0].includes('Root.WithOrg.Client.Events'));
        expect(state).toBeUndefined();
    });
});
