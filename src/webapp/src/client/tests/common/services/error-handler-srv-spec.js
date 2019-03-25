describe('errorHandler', function() {
    var service, $q;

    beforeEach(module('simoonaApp.Common'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('localeSrv', servicesMocks.localeSrv);
            $provide.value('notifySrv', servicesMocks.notifySrv);
            $provide.value('errorCodeMessages', servicesMocks.errorCodeMessages);
            $provide.value('lodash', servicesMocks.lodash);
        });
    });

    beforeEach(inject(function(_$q_, _errorHandler_) {
        $q = _$q_;
        service = _errorHandler_;

        spyOn(servicesMocks.notifySrv, 'error').and.callFake(function() {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        });
    }));

    it('should be defined and have handleErrorMessage method inside', function() {
        expect(service).toBeDefined();

        expect(service.handleErrorMessage).toBeDefined();
    });

    it('should handle simple error code message', function() {
        var error = {
            data: {
                message: '200'
            }
        };
        service.handleErrorMessage(error);

        expect(servicesMocks.notifySrv.error).toHaveBeenCalled();
        expect(servicesMocks.notifySrv.error).toHaveBeenCalledWith('errorCodeMessages.invalidStartDate');
    });

    it('should be able to insert value in error message', function() {
        var error = {
            data: {
                message: 202,
                a: 10
            }
        };
        service.handleErrorMessage(error);

        expect(servicesMocks.notifySrv.error).toHaveBeenCalled();
        expect(servicesMocks.notifySrv.error).toHaveBeenCalledWith('errorCodeMessages.requiredError');
    });

    it('should show old error message', function() {
        var error = {
            message: 'Error message',
            data: {}
        };
        service.handleErrorMessage(error);

        expect(servicesMocks.notifySrv.error).toHaveBeenCalled();
        expect(servicesMocks.notifySrv.error).toHaveBeenCalledWith('Error message');
    });

});
