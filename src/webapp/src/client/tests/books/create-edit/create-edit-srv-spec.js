describe('bookCreateEditService', function () {
    var service;

    beforeEach(module('simoonaApp.Books'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('authService', bookMocks.authService);
        });
    });
    beforeEach(inject(function (_bookCreateEditService_) {
        service = _bookCreateEditService_;
    }));

    it('should be defined and have filterOffices, getActionColumn, getCurrentOffice, getOfficeQuantities methods', function () {
        expect(service).toBeDefined();

        expect(service.filterOffices).toBeDefined();
        expect(service.getActionColumn).toBeDefined();
        expect(service.getCurrentOffice).toBeDefined();
        expect(service.getOfficeQuantities).toBeDefined();
    });

    describe('filterOffices', function () {
        it('should filter out offices with empty books', function () {
            var offices = service.filterOffices(bookMocks.bookQuantityOffices);

            expect(offices.length).toEqual(2);
        });
    });

    describe('getActionColumn', function () {
        it('should show action column if book is taken', function () {
            var showColumn = service.getActionColumn([{userId: 1, returned: false}], {userId: 1});

            expect(showColumn).toBe(true);
        });

        it('should hide action column if book was not taken', function () {
            var showColumn = service.getActionColumn([{userId: 1, returned: true}], {userId: 1});

            expect(showColumn).toBe(false);
        });
    });

    describe('getCurrentOffice', function () {
        it('should return empty string if there is no such office', function () {
            var office = service.getCurrentOffice(bookMocks.offices, '10000');

            expect(office).not.toBeDefined();
        });

        it('should return office', function () {
            var office = service.getCurrentOffice(bookMocks.offices, '1');

            expect(office).toBe(bookMocks.offices[0]);
        });
    });

    describe('getOfficeQuantities', function () {
        it('should set book quantities to 0', function () {
            var offices = service.getOfficeQuantities(bookMocks.officeQuantities, null);

            expect(offices).toEqual(bookMocks.officeQuanititesNoBooksExpectedResult);
        });

        it('should set correct book quantities', function () {
            var bookQuantities = [{officeId: 1, bookQuantity: 5}, {officeId:2, bookQuantity: 10}];
            var offices = service.getOfficeQuantities(bookMocks.officeQuantities, bookQuantities);

            expect(offices).toEqual(bookMocks.officeQuanititesWithBooksExpectedResult);
        });
    });
});
