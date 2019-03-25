describe('aceBirthdays', function () {
    var ctrl, scope, $rootScope, element, service;

    beforeEach(module('simoonaApp.Birthdays'));

    beforeEach(function () {
        module(function ($provide) {
            $provide.value('authService', birthdaysMocks.authService);
            $provide.value('$resource', birthdaysMocks.$resource);
            $provide.value('notifySrv', birthdaysMocks.notifySrv);
            $provide.value('endPoint', birthdaysMocks.endPoint);
            $provide.value('Analytics', birthdaysMocks.Analytics);
            $provide.value('smallAvatarThumbSettings', birthdaysMocks.smallAvatarThumbSettings);
            $provide.value('birthdaysRepository', birthdaysMocks.birthdaysRepository);            
        });
    });

    beforeEach(inject(function (_$q_) {
        spyOn(birthdaysMocks.birthdaysRepository, 'getUsers').and.callFake(function () {
            var deferred = _$q_.defer();
            deferred.resolve();
            return deferred.promise;
        });
    }));

    beforeEach(inject(function ($compile, $rootScope, $templateCache) {
        scope = $rootScope.$new();

        $templateCache.put('app/birthdays/birthdays.html', '<div></div>');
        element = angular.element('<ace-birthdays></ace-birthdays>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceBirthdays');
    }));

    it('should be initialized', function () {
        expect(birthdaysMocks.birthdaysRepository.getUsers).toHaveBeenCalled();
    });
});
