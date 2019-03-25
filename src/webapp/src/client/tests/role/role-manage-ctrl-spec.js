describe('rolesManageController', function () {
    var service, ctrl, $q, scope, state, $controller;

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
            $provide.value('role', roleMocks.role);
            $provide.value('errorHandler', roleMocks.errorHandler);
            $provide.value('$state', roleMocks.$state);
            $provide.value('controllers', roleMocks.controllers);
        });
    });

    beforeEach(inject(function (_roleRepository_, _$q_, _$state_) {
        service = _roleRepository_;
        $q = _$q_;
        state = _$state_;
    }));

    beforeEach(inject(function (_$controller_, _$rootScope_) {
        scope = _$rootScope_.$new();
        $controller = _$controller_;
        $rootScope = _$rootScope_;
        ctrl = _$controller_('rolesManageController', {
            $rootScope: _$rootScope_,
            $scope: scope,
            roleRepository: service
        });

        _$rootScope_.$digest();
    }));

    it('should be defined', function () {
        expect(ctrl).toBeDefined();
    })

    it('should update role', function () {
        state.current.name = 'Root.WithOrg.Admin.Roles.Edit';
        spyOn(roleMocks.notifySrv, 'success');
        spyOn(service, 'update').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        scope.onSave();
        scope.$digest();

        expect(service.update).toHaveBeenCalledWith(roleMocks.role);
        expect(roleMocks.notifySrv.success).toHaveBeenCalled();
    })

    it('should save role', function () {
        state.current.name = 'Root.WithOrg.Admin.Roles.Create';
        spyOn(roleMocks.notifySrv, 'success');
        spyOn(state, 'go');
        spyOn(service, 'create').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        scope.onSave();
        scope.$digest();

        expect(service.create).toHaveBeenCalledWith(roleMocks.role);
        expect(state.go).toHaveBeenCalledWith('^.List');
        expect(roleMocks.notifySrv.success).toHaveBeenCalled();
    })

    it('should set title scope for role editing', function () {
        spyOn(state, 'includes').and.callFake(function (stateName) {
            if (roleMocks.roleEditState === stateName) {
                return true;
            }
            return false;
        });

        let roleScope = $rootScope.$new();
        let controller = $controller('rolesManageController', {
            $rootScope: $rootScope,
            $scope: roleScope,
            roleRepository: service,
            $state: state
        });
        roleScope.$digest();

        expect(state.includes).toHaveBeenCalled();
        expect(state.includes).toHaveBeenCalledWith(roleMocks.roleEditState);
        expect(roleScope.titleEdit).toBeTruthy();
        expect(roleScope.titleCreate).toBeFalsy();
    })

    it('should set title scope for role creation', function () {
        spyOn(state, 'includes').and.callFake(function (stateName) {
            if (roleMocks.roleCreateState === stateName) {
                return true;
            }
            return false;
        });

        let roleScope = $rootScope.$new();
        let controller = $controller('rolesManageController', {
            $rootScope: $rootScope,
            $scope: roleScope,
            roleRepository: service,
            $state: state
        });
        roleScope.$digest();

        expect(state.includes).toHaveBeenCalled();
        expect(state.includes).toHaveBeenCalledWith(roleMocks.roleCreateState);
        expect(roleScope.titleEdit).toBeFalsy();
        expect(roleScope.titleCreate).toBeTruthy();
    })

    it('should load users', function () {
        spyOn(service, 'getUsersForAutoComplete');

        scope.loadUsers('search');

        expect(service.getUsersForAutoComplete).toHaveBeenCalled();
        expect(service.getUsersForAutoComplete).toHaveBeenCalledWith('search');
    })


    it('should set permission groups', function () {
        let controllers = [
            {
                name: 'account'
            },
            {
                name: 'admin'
            }
        ];
        let role = [];

        let controller = $controller('rolesManageController', {
            $rootScope: $rootScope,
            $scope: scope,
            roleRepository: service,
            $state: state,
            controllers: controllers,
            role: role
        });
      
        expect(scope.permissionGroups).toBe(controllers);
    })
});