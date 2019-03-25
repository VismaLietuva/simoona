describe('bookRepository', function () {
    var service;

    beforeEach(module('simoonaApp.Books'));
 
    beforeEach(function() {
        module(function($provide) {
            $provide.value('$resource', bookMocks.$resource);
            $provide.value('endPoint', bookMocks.endPoint);
            $provide.value('authService', bookMocks.authService);
        });
    });

    beforeEach(inject(function (_bookRepository_) {
        service = _bookRepository_;
    }));

    it('should be defined and have takeBook, returnBook, getFilteredBooks methods', function () {
        expect(service).toBeDefined();

        expect(service.takeBook).toBeDefined();
        expect(service.returnBook).toBeDefined();
        expect(service.getFilteredBooks).toBeDefined();
    });
});
