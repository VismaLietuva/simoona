describe('rolesListController', function () {
    var service, ctrl, $q, scope;

    beforeEach(module('simoonaApp.Role'));

    beforeEach(function () {
        module(function ($provide) {
            $provide.value('authService', roleMocks.authService);
            $provide.value('$resource', roleMocks.$resource);
            $provide.value('notifySrv', roleMocks.notifySrv);
            $provide.value('endPoint', roleMocks.endPoint);
            $provide.value('roles', roleMocks.roles);
            $provide.value('$advancedLocation', roleMocks.$advancedLocation);
            $provide.value('$uibModal', roleMocks.$uibModal);
            $provide.value('$translate', roleMocks.$translate);
            $provide.value('localeSrv', roleMocks.localeSrv);
        });
    });

    beforeEach(inject(function (_roleRepository_, _$q_) {
        service = _roleRepository_;
        $q = _$q_;
    }));

    beforeEach(inject(function (_$controller_, _$rootScope_) {
        scope = _$rootScope_.$new();
        ctrl = _$controller_('rolesListController', {
            $rootScope: _$rootScope_,
            $scope: scope,
            roleRepository: service
        });

        _$rootScope_.$digest();
    }));

    beforeEach(function () {
        spyOn(service, 'getPaged').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve({
                roles: roleMocks.roles
            });
            return deferred.promise;
        });
    });

    it('should be defined', function () {
        expect(ctrl).toBeDefined();
    })

    it('should get items', function () {
        scope.onSearch();

        expect(service.getPaged).toHaveBeenCalled();
    })

    it('should delete items', function () {
        spyOn(service, 'deleteItem').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve({
                roles: roleMocks.roles
            });
            return deferred.promise;
        });

        spyOn(roleMocks.notifySrv, 'success');

        scope.deleteItem({
            id: 1
        });
        scope.$digest();

        expect(service.deleteItem).toHaveBeenCalled();
        expect(service.deleteItem).toHaveBeenCalledWith(1);
        expect(roleMocks.notifySrv.success).toHaveBeenCalled();
    })

    it('should sort', function () {
        scope.onSort('sort', 'dir');

        expect(service.getPaged).toHaveBeenCalled();
        expect(service.getPaged).toHaveBeenCalledWith({
            sort: 'sort',
            dir: 'dir',
            page: 1
        });
    })

    it('should apply filter to sort', function () {
        scope.filter.s = 'filter';
        scope.onSort('sort', 'dir');

        expect(service.getPaged).toHaveBeenCalled();
        expect(service.getPaged).toHaveBeenCalledWith({
            sort: 'sort',
            dir: 'dir',
            page: 1,
            s: 'filter'
        });
    })
});