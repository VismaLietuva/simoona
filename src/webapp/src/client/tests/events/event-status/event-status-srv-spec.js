describe('eventStatusService', function () {
    var service;

    beforeEach(module('simoonaApp.Events'))
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('eventStatus', eventsMocks.eventStatus);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        })
    });
    beforeEach(inject(function (_eventStatusService_) {
        service = _eventStatusService_;
    }));

    it('should be defined and have getEventStatus method', function () {
        expect(service).toBeDefined();

        expect(service.getEventStatus).toBeDefined();
    });

    describe('getEventStatus', function () {
        it('should return inProgress if event has started', function() {
            var result = service.getEventStatus(eventsMocks.inProgressEvent);

            expect(result).toBe(eventsMocks.eventStatus.InProgress);
        });

        it('should return Finished if event has finished', function() {
            var result = service.getEventStatus(eventsMocks.finishedEvent);

            expect(result).toBe(eventsMocks.eventStatus.Finished);
        });

        it('should return RegistrationIsClosed if event registration has passed and event is about to start', function() {
            var result = service.getEventStatus(eventsMocks.registrationIsClosedEvent);

            expect(result).toBe(eventsMocks.eventStatus.RegistrationIsClosed);
        });

        it('should return Full if event is full and user is not participanting in event', function() {
            var result = service.getEventStatus(eventsMocks.fullEvent);

            expect(result).toBe(eventsMocks.eventStatus.Full);
        });

        it('should return Join if user is not participanting in event and registration deadline is ahead', function() {
            var result = service.getEventStatus(eventsMocks.joinEvent);

            expect(result).toBe(eventsMocks.eventStatus.Join);
        });
    });
});
