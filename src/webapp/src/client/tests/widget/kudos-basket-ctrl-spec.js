describe('kudosBasketController', function () {
    var controller, $rootScope, $q, service;

    beforeEach(module('simoonaApp.Widget.KudosBasket'));

    beforeEach(function () {
        module(function ($provide) {
            $provide.value('appConfig', kudosBasketMocks.appConfig);
            $provide.value('endPoint', kudosBasketMocks.endPoint);
            $provide.value('notifySrv', kudosBasketMocks.notifySrv);
            $provide.value('$state', kudosBasketMocks.$state);
            $provide.value('$resource', kudosBasketMocks.$resource);
        });
    });

    beforeEach(inject(function (_kudosBasketRepository_) {
        service = _kudosBasketRepository_;
    }));


    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(service, 'getKudosBasket').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(kudosBasketMocks.kudosBasketData);
            return deferred.promise;
        });

        spyOn(service, 'getDonations').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve({});
            return deferred.promise;
        });

        spyOn(kudosBasketMocks.notifySrv, 'success').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        spyOn(kudosBasketMocks.notifySrv, 'error').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });

        spyOn(kudosBasketMocks.$state, 'go').and.callThrough();
    }));

    beforeEach(inject(function (_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;

        controller = _$controller_('KudosBasketController', {
            $rootScope: $rootScope,
            kudosBasketRepository: service
        });

        $rootScope.$digest();
    }));

    it('should be defined', function () {
        expect(controller).toBeDefined();
    });

    describe('saveKudosBasket', function () {
        it('should update kudos basket', function () {
            spyOn(service, 'editKudosBasket').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });

            controller.saveKudosBasket();
            $rootScope.$digest();

            expect(service.editKudosBasket).toHaveBeenCalledWith(kudosBasketMocks.kudosBasketData);
            expect(kudosBasketMocks.notifySrv.success).toHaveBeenCalled();
        });

        it('should save kudos basket', function () {
            controller.kudosBasketData = {};
            spyOn(service, 'createNewBasket').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });

            controller.saveKudosBasket();
            $rootScope.$digest();

            expect(kudosBasketMocks.notifySrv.success).toHaveBeenCalled();
        });
    });

    describe('deleteKudosBasket', function () {
        it('should show error', function () {
            spyOn(service, 'deleteKudosBasket').and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({
                    data: {
                        message: 'Error'
                    }
                });
                return deferred.promise;
            });

            controller.deleteKudosBasket();
            $rootScope.$digest();

            expect(kudosBasketMocks.notifySrv.error).toHaveBeenCalled();
        });

        it('should delete kudos basket', function () {
            spyOn(service, 'deleteKudosBasket').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });

            controller.deleteKudosBasket();
            $rootScope.$digest();

            expect(kudosBasketMocks.notifySrv.success).toHaveBeenCalled();
        });
    });

    it('should cancel kudos basket', function () {
        controller.cancelKudosBasket();
        $rootScope.$digest();

        expect(kudosBasketMocks.$state.go).toHaveBeenCalled();
        expect(kudosBasketMocks.$state.go).toHaveBeenCalledWith(kudosBasketMocks.appConfig.homeStateName, {}, { reload: true });
    });
});
