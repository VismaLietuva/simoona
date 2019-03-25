describe('organizationalStructureController', function () {
    var $rootScope, orgStructScope, orgStructCtrl, mockStateProvider;

    beforeEach(angular.mock.module(function ($windowProvider) {
        $windowProvider.$get = organizationalStructureMocks.$windowProvider.$get;
    }));

    beforeEach(function () {
        module('ui.router', function ($stateProvider) {
            mockStateProvider = $stateProvider;
            spyOn(mockStateProvider, 'state').and.callThrough();
        });
    });

    beforeEach(module('simoonaApp.OrganizationalStructure'));

    beforeEach(function() {
        module(function($provide) {
            $provide.value('lodash', organizationalStructureMocks.lodash);
            $provide.value('$translate', organizationalStructureMocks.$translate);
            $provide.value('authService', organizationalStructureMocks.authService);
        });
    });
    beforeEach(inject(function (_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;
        orgStructScope = $rootScope.$new();

        orgStructCtrl = _$controller_('organizationalStructureController', {
            $rootScope: orgStructScope,
            resource: organizationalStructureMocks.resource
        });

        orgStructScope.$digest();
    }));

    it('should initialize necessary methods and variables', function () {
        expect(orgStructCtrl).toBeDefined();

        expect(orgStructCtrl.resetOrgTree).toBeDefined();
    });

    it('if resetOrgTree method is called should set resetTreeFn to the given parameter', function () {
        orgStructCtrl.resetOrgTree(organizationalStructureMocks.expectedResult);

        expect(orgStructCtrl.resetTreeFn).toEqual(organizationalStructureMocks.expectedResult);
    });

    it('should not register premium routes', function () {
        let state = mockStateProvider.state.calls.allArgs().find(s => s[0].includes('Root.WithOrg.Client.OrganizationalStructure'));
        expect(state).toBeUndefined();
    });
    
});
