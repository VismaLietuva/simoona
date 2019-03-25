describe('eventsService', function () {
    var service;

    beforeEach(module('simoonaApp.Events'))
    beforeEach(function() {
        module(function($provide) {
            $provide.value('$resource', eventsMocks.$resource);
            $provide.value('endPoint', eventsMocks.endPoint);
            $provide.value('authService', eventsMocks.authService);
            $provide.value('$windowProvider', eventsMocks.$windowProvider);
        });
    });
    beforeEach(inject(function (_eventRepository_) {
        service = _eventRepository_;
    }));

    it('should be defined and have getEventTypes and getEventsByType methods', function () {
        expect(service).toBeDefined();

        expect(service.getEventTypes).toBeDefined();
        expect(service.getEventsByType).toBeDefined();
    });

    describe('getEventTypes', function () {
        beforeEach(function () {
            spyOn(service, 'getEventTypes').and.callFake(function () {
                return eventsMocks.eventTypes;
            });
        });

        it('should return array composed of objects with id and name properties', function() {
            var result = service.getEventTypes();

            expect(result.length).toBe(3);

            for (var i = 0; result.length > i;i++) {
                expect(result[i].id).toBeDefined();
                expect(result[i].name).toBeDefined();
            }
        });
    });

    describe('getEventsByType', function () {
        beforeEach(function () {
            spyOn(service, 'getEventsByType').and.callFake(function (typeId) {
                return eventsMocks.getEventsByType(typeId);
            });
        });

        it('should return empty array if there is no such event type in list', function() {
            var result = service.getEventsByType(-1);

            expect(result.length).toBe(0);
        });

        it('should return all events if there is no specified parameter', function() {
            var result = service.getEventsByType();

            expect(result).toBe(eventsMocks.events);
        });

        it('should return events with specified type parameter', function() {
            var result = service.getEventsByType(2);

            for (var i = 0; result.length > i; i++) {
                expect(result[i].typeId).toEqual(2);
            }
        });
    });
});
