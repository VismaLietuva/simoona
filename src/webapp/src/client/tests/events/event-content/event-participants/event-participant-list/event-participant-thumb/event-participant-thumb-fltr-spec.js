describe('participantThumb', function () {
    var $filter, endPoint;

    beforeEach(module('simoonaApp.Events'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('endPoint', eventsMocks.endPoint);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });
    beforeEach(inject(function (_$filter_, _endPoint_, _authService_) {
        endPoint = _endPoint_;
        $filter = _$filter_('participantThumb');
        authService = _authService_;
    }));

    it('should return default image', function () {
        var image = $filter();

        expect(image).toEqual('/images/participantThumb.png');
    });

    it('should return image with specified width, height and mode', function () {
        var image = $filter(eventsMocks.participantInput, eventsMocks.participantThumb);

        expect(image).toEqual(endPoint + '/storage/' + authService.identity.organizationName + '/image.jpg?width=100&height=100&mode=crop');
    });
});
