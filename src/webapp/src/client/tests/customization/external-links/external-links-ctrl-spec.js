describe('externalLinksController', function() {
    var $rootScope, scope, ctrl, $q;

    beforeEach(module('simoonaApp.Customization.ExternalLinks'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('lodash', customizationMocks.lodash);
            $provide.value('authService', customizationMocks.authService);
        });
    });
    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(customizationMocks.externalLinksRepository, 'getExternalLinks').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(customizationMocks.externalLinks);
            return deferred.promise;
        });
        spyOn(customizationMocks.externalLinksRepository, 'postExternalLinks').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(customizationMocks.externalLinksUpdateObject);
            return deferred.promise;
        });
        spyOn(customizationMocks.$state, 'go');
    }));

    beforeEach(inject(function(_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;
        scope = $rootScope.$new();

        ctrl = _$controller_('externalLinksController', {
            $state: customizationMocks.$state,
            externalLinksRepository : customizationMocks.externalLinksRepository,
            notifySrv: customizationMocks.notifySrv,
            errorHandler:customizationMocks.errorHandler
        });

        $rootScope.$digest();

    }));

    it('should be initialized', function() {
        expect(ctrl).toBeDefined();
    });

    it('should get link list', function() {
        expect(customizationMocks.externalLinksRepository.getExternalLinks).toHaveBeenCalled();
        expect(ctrl.externalLinks).toEqual(customizationMocks.externalLinks);
    });

    it('should add empty link to list', function() {
        ctrl.addLink();
        $rootScope.$digest();
        expect(ctrl.externalLinks).toEqual(customizationMocks.externalLinksWithAddedLink);
    });

    it('should delete link and add link id to array', function() {
        var deletedId = ctrl.externalLinks[1].id;
        ctrl.deleteLink(1);
        $rootScope.$digest();
        expect(ctrl.externalLinks).toEqual(customizationMocks.externalLinksAfterDelete);
        expect(ctrl.linksToDelete[0]).toEqual(deletedId);
    });
    it('should save link list and redirect', function() {
        ctrl.saveLinks();
        $rootScope.$digest();
        expect(customizationMocks.externalLinksRepository.postExternalLinks).toHaveBeenCalled();
        expect(customizationMocks.$state.go).toHaveBeenCalled();
    });

});
