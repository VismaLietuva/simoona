describe('booksController', function () {
    var $rootScope, $q, scope, ctrl;

    beforeEach(module('simoonaApp.Books'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('authService', bookMocks.authService);
        });
    });
    beforeEach(inject(function (_$q_) {
        $q = _$q_;

        spyOn(bookMocks.bookRepository, 'getAllOffices').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(bookMocks.offices);
            return deferred.promise;
        });

        spyOn(bookMocks.bookRepository, 'getFilteredBooks').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(bookMocks.books1);
            return deferred.promise;
        });

        spyOn(bookMocks.$translate, 'instant').and.callFake(function (key) {
            return key;
        });
    }));
    beforeEach(inject(function (_$controller_,  _$rootScope_) {
        $rootScope = _$rootScope_;
        scope = $rootScope.$new();

        ctrl = _$controller_('booksController', {
            $rootScope: _$rootScope_,
            $translate: bookMocks.$translate,
            notifySrv: bookMocks.notifySrv,
            authService: bookMocks.authService,
            bookRepository: bookMocks.bookRepository,
            localeSrv: bookMocks.localeSrv
        });

        $rootScope.$digest();
    }));

    it('should be initialized', function () {
        expect(ctrl).toBeDefined();

        expect(bookMocks.bookRepository.getAllOffices).toHaveBeenCalled();
        expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalled();
        expect(ctrl.filter).toEqual({page: 1, search: '', officeId: 1});
    });

    describe('takeBook', function () {
        beforeEach(function () {
            spyOn(bookMocks.bookRepository, 'takeBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });

            spyOn(bookMocks.notifySrv, 'success');

            ctrl.takeBook(bookMocks.books1.entries[0]);

            $rootScope.$digest();
        });

        it('should be able to take book', function () {
            var book = bookMocks.books1.entries[0];
            expect(book.pending).toBe(true);
            expect(bookMocks.bookRepository.takeBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.takeBook).toHaveBeenCalledWith(book.id);
        });

        it('should display success message', function () {
            var book = bookMocks.books1.entries[0];
            var message = String.format('books.successTaken', book.title, book.author);

            expect(bookMocks.notifySrv.success).toHaveBeenCalled();
            expect(bookMocks.notifySrv.success).toHaveBeenCalledWith(message);
        });
    });

    describe('returnBook', function () {
        beforeEach(function () {
            spyOn(bookMocks.bookRepository, 'returnBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });

            spyOn(bookMocks.notifySrv, 'success');

            ctrl.returnBook(bookMocks.books1.entries[0]);

            $rootScope.$digest();
        });

        it('should be able to return book', function () {
            var book = bookMocks.books1.entries[0];

            expect(bookMocks.bookRepository.returnBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.returnBook).toHaveBeenCalledWith(book.id);
        });

        it('should display success message', function () {
            var book = bookMocks.books1.entries[0];
            var message = String.format('books.successReturned', book.title, book.author);

            expect(bookMocks.notifySrv.success).toHaveBeenCalled();
            expect(bookMocks.notifySrv.success).toHaveBeenCalledWith(message);
        });
    });

    describe('getFilteredBooks', function () {
        it('should get books pagination information', function () {
            expect(ctrl.books.page).toEqual(1);
            expect(ctrl.books.pageSize).toBeDefined();
            expect(ctrl.books.itemCount).toBeDefined();
            expect(ctrl.books.pageCount).toBeDefined();
        });

        it('should get books from service with specified filter', function () {
            expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalledWith({page: 1, search: '', officeId: 1});
            expect(ctrl.books).toEqual(bookMocks.books1);
        });
    });

    describe('searchFilter', function () {
        var oldFilter = {};
        beforeEach(function () {
            oldFilter = ctrl.filter;
            ctrl.searchFilter('text');
        });

        it('should set filters page value to 1', function () {
            expect(ctrl.filter.page).toEqual(1);
        });

        it('should set filters search value to specified string', function () {
            expect(ctrl.filter.search).toEqual('text');
        });

        it('should not modify filters officeId', function () {
            expect(ctrl.filter.officeId).toEqual(oldFilter.officeId);
        });

        it('should call getFilteredBooks method with new filter', function () {
            expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalled();
            expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalledWith(ctrl.filter);
        });
    });

    describe('changePage', function () {
        var oldFilter = {};
        beforeEach(function () {
            oldFilter = ctrl.filter;
            ctrl.changePage(2);
        });

        it('should set filters page value to new one', function () {
            expect(ctrl.filter.page).toEqual(2);
        });

        it('should not modify filters search string and officeId', function () {
            expect(ctrl.filter.search).toEqual(oldFilter.search);
            expect(ctrl.filter.officeId).toEqual(oldFilter.officeId);
        });

        it('should call getFilteredBooks method with new filter', function () {
            expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalled();
            expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalledWith(ctrl.filter);
        });
    });

    describe('toggleOffice', function () {
        var oldFilter = {};
        beforeEach(function () {
            oldFilter = ctrl.filter;
            ctrl.toggleOffice(bookMocks.offices[1]);
        });

        it('should set filters page value to new one', function () {
            expect(ctrl.filter.page).toEqual(1);
        });

        it('should not modify filters search string', function () {
            expect(ctrl.filter.search).toEqual(oldFilter.search);
        });

        it('should set filters officeId value to new one', function () {
            expect(ctrl.filter.officeId).toEqual(bookMocks.offices[1].id);
        });

        it('should call getFilteredBooks method with new filter', function () {
            expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalled();
            expect(bookMocks.bookRepository.getFilteredBooks).toHaveBeenCalledWith(ctrl.filter);
        });
    });
});
