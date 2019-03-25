describe('externalLinksRepository', function () {
    var service;

    beforeEach(module('simoonaApp.Customization.ExternalLinks'));

    beforeEach(function() {
        module(function($provide) {
            $provide.value('$resource', customizationMocks.$resource);
            $provide.value('endPoint', customizationMocks.endPoint);
            $provide.value('authService', customizationMocks.authService);
            $provide.value('lodash', customizationMocks.lodash);
        });
    });

    beforeEach(inject(function (_externalLinksRepository_) {
        service = _externalLinksRepository_;
    }));

    it('should be defined', function() {
        expect(service).toBeDefined();
        expect(service.getExternalLinks).toBeDefined();
        expect(service.postExternalLinks).toBeDefined();
    });
});
