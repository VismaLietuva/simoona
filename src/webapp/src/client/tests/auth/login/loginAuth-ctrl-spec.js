describe('loginAuthController', function () {
    let $rootScope, scope, ctrl, $q, $timeout, $controller;

    beforeEach(module('simoonaApp.Auth'));

    beforeEach(function () {
        module(function ($provide) {
            $provide.value('localStorageService', loginAuthMocks.localStorageService);
            $provide.value('$resource', loginAuthMocks.$resource);
            $provide.value('appConfig', loginAuthMocks.appConfig);
            $provide.value('lodash', loginAuthMocks.lodash);
            $provide.value('localeSrv', loginAuthMocks.localeSrv);
            $provide.value('endPoint', loginAuthMocks.endPoint);
            $provide.value('notifySrv', loginAuthMocks.notifySrv);
            $provide.value('errorHandler', loginAuthMocks.errorHandler);
            $provide.value('authService', loginAuthMocks.authService);
        });
    });

    beforeEach(function () {
        spyOn(loginAuthMocks.authService, 'getExternalLogins').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve([]);
            return deferred.promise;
        });
    })

    beforeEach(inject(function (_$controller_, _$rootScope_, _$q_, _$timeout_) {
        $rootScope = _$rootScope_;
        scope = $rootScope.$new();
        $q = _$q_;
        $timeout = _$timeout_;
        $controller = _$controller_;

        ctrl = _$controller_('loginAuthController', {
            $rootScope: _$rootScope_,
            $scope: scope,
            $timeout: _$timeout_
        });

        $rootScope.$digest();
    }));

    it('should be defined', function () {
        $timeout.flush()
        expect(ctrl).toBeDefined();
    });

    describe('init', function () {
        describe('getExternalLogins', function () {
            it('should show error', function () {
                loginAuthMocks.authService.getExternalLogins = jasmine.createSpy().and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.reject('Error Message');
                    return deferred.promise;
                });
                spyOn(loginAuthMocks.errorHandler, 'handleErrorMessage');

                let externalLoginScope = $rootScope.$new();
                let controller = $controller('loginAuthController', {
                    $rootScope: $rootScope,
                    $scope: externalLoginScope
                });
                externalLoginScope.$digest();

                expect(loginAuthMocks.errorHandler.handleErrorMessage).toHaveBeenCalledWith('Error Message');
            });
            it('should set Google provider', function () {
                loginAuthMocks.authService.getExternalLogins = jasmine.createSpy().and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.resolve(['Google']);
                    return deferred.promise;
                });
              
                let externalLoginScope = $rootScope.$new();
                let controller = $controller('loginAuthController', {
                    $rootScope: $rootScope,
                    $scope: externalLoginScope
                });
                externalLoginScope.$digest();
                
                expect(controller.isGoogle).toBeTruthy();
                expect(controller.isFacebook).toBeFalsy();
            });
            it('should set Facebook provider', function () {
                loginAuthMocks.authService.getExternalLogins = jasmine.createSpy().and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.resolve(['Facebook']);
                    return deferred.promise;
                });
              
                let externalLoginScope = $rootScope.$new();
                let controller = $controller('loginAuthController', {
                    $rootScope: $rootScope,
                    $scope: externalLoginScope
                });
                externalLoginScope.$digest();
                
                expect(controller.isGoogle).toBeFalsy();
                expect(controller.isFacebook).toBeTruthy();
            });
        });

    });
});