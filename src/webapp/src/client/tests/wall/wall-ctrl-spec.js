describe('wallController', function () {
    var ctrl, scope, $rootScope, $q;

    beforeEach(module('simoonaApp.Wall'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('authService', wallMocks.authService);
        });
    });

    beforeEach(inject(function (_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;
        scope = $rootScope.$new();

        ctrl = _$controller_('wallController', {
            $rootScope: _$rootScope_,
            $scope: $rootScope
        });

        $rootScope.$digest();
    }));

    it('should be initialized', function () {
        expect($rootScope.pageTitle).toEqual('wall.wallTitle');
    });
});
