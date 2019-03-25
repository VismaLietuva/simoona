describe('booksCreateEditController', function () {
    var $rootScope, $controller, $q, addScope, editScope, addCtrl, editCtrl;

    beforeEach(module('simoonaApp.Books'));
    beforeEach(function () {
        module(function ($provide) {
            $provide.value('bookSettings', bookMocks.bookSettings);
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

        spyOn(bookMocks.bookRepository, 'getBookDetails').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(bookMocks.bookInfo);
            return deferred.promise;
        });

        spyOn(bookMocks.bookRepository, 'getBookDetailsForAdministrator').and.callFake(function () {
            var deferred = $q.defer();
            deferred.resolve(bookMocks.bookInfoForAdmin);
            return deferred.promise;
        });
    }));
    beforeEach(inject(function (_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;
        $controller = _$controller_;
        addScope = $rootScope.$new();

        addCtrl = _$controller_('booksCreateEditController', {
            $rootScope: addScope,
            $scope: addScope,
            $state: bookMocks.addState,
            $stateParams: {id: 1},
            $translate: bookMocks.$translate,
            notifySrv: bookMocks.notifySrv,
            authService: bookMocks.authService,
            bookRepository: bookMocks.bookRepository,
            bookCreateEditService: bookMocks.bookCreateEditService,
            bookSettings: bookMocks.bookSettings,
            localeSrv: bookMocks.localeSrv
        });

        addScope.$digest();
    }));

    beforeEach(inject(function (_$controller_, _$rootScope_) {
        $rootScope = _$rootScope_;
        $controller = _$controller_;
        editScope = $rootScope.$new();

        editCtrl = _$controller_('booksCreateEditController', {
            $rootScope: editScope,
            $scope: editScope,
            $state: bookMocks.editState,
            $stateParams: {id: 1},
            $translate: bookMocks.$translate,
            notifySrv: bookMocks.notifySrv,
            authService: bookMocks.authService,
            bookRepository: bookMocks.bookRepository,
            bookCreateEditService: bookMocks.bookCreateEditService,
            bookSettings: bookMocks.bookSettings,
            localeSrv: bookMocks.localeSrv
        });

        editScope.$digest();
    }));

    describe('ínit', function () {
        describe('add book', function () {
            it('should be initialized', function () {
                expect(editCtrl).toBeDefined();

                expect(bookMocks.bookRepository.getAllOffices).toHaveBeenCalled();
                expect(addScope.pageTitle).toEqual('books.addBookTitle');
            });

            it('should not set office', function () {
                expect(addCtrl.currentOffice).toBeUndefined();
            });
        });

        describe('edit book', function () {
            it('should be initialized', function () {
                expect(editCtrl).toBeDefined();

                expect(bookMocks.bookRepository.getAllOffices).toHaveBeenCalled();
                expect(editScope.pageTitle).toEqual('books.editBookTitle');
            });

            it('should set office title', function () {
                expect(editCtrl.currentOffice).toEqual('Vilnius');
            });
        });
    });

    describe('getBookDetails', function () {
        it('user should not see office information', function () {
            var usrScope = $rootScope.$new();

            usrCtrl = $controller('booksCreateEditController', {
                $rootScope: editScope,
                $scope: editScope,
                $state: bookMocks.editState,
                $stateParams: {id: 1},
                $translate: bookMocks.$translate,
                notifySrv: bookMocks.notifySrv,
                authService: bookMocks.userAuthService,
                bookRepository: bookMocks.bookRepository,
                bookCreateEditService: bookMocks.bookCreateEditService,
                bookSettings: bookMocks.bookSettings,
                localeSrv: bookMocks.localeSrv
            });
            usrScope.$digest();

            expect(bookMocks.bookRepository.getBookDetails).toHaveBeenCalled();
            expect(usrCtrl.book).toBeDefined();
            expect(usrCtrl.book.quantityByOffice).not.toBeDefined();
        });

        it('administrator should see office information', function () {
            var admScope = $rootScope.$new();

            admCtrl = $controller('booksCreateEditController', {
                $rootScope: editScope,
                $scope: editScope,
                $state: bookMocks.editState,
                $stateParams: {id: 1},
                $translate: bookMocks.$translate,
                notifySrv: bookMocks.notifySrv,
                authService: bookMocks.authService,
                bookRepository: bookMocks.bookRepository,
                bookCreateEditService: bookMocks.bookCreateEditService,
                bookSettings: bookMocks.bookSettings,
                localeSrv: bookMocks.localeSrv
            });
            admScope.$digest();

            expect(bookMocks.bookRepository.getBookDetailsForAdministrator).toHaveBeenCalled();
            expect(admCtrl.book).toBeDefined();
            expect(admCtrl.book.quantityByOffice).toBeDefined();
        });
    });

    describe('savebook', function () {
        describe('add book', function () {
            it('should throw error that book already exists', function () {
                spyOn(bookMocks.bookRepository, 'createBook').and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.reject({
                        data: {
                            message: '101'
                        }
                    });
                    return deferred.promise;
                });
                spyOn(bookMocks.notifySrv, 'error');

                addCtrl.saveBook({});
                addScope.$digest();

                expect(bookMocks.bookRepository.createBook).toHaveBeenCalled();
                expect(bookMocks.notifySrv.error).toHaveBeenCalled();
                expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('books.bookAlreadyExists');
            });

            it('should throw any other error message', function () {
                spyOn(bookMocks.bookRepository, 'createBook').and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.reject({
                        data: {
                            message: 'Error happened'
                        }
                    });
                    return deferred.promise;
                });
                spyOn(bookMocks.notifySrv, 'error');

                addCtrl.saveBook({});
                addScope.$digest();

                expect(bookMocks.bookRepository.createBook).toHaveBeenCalled();
                expect(bookMocks.notifySrv.error).toHaveBeenCalled();
                expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('Error happened');
            });

            it('should create book, redirect to list page and display succesful message', function () {
                spyOn(bookMocks.bookRepository, 'createBook').and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
                spyOn(bookMocks.addState, 'go');
                spyOn(bookMocks.notifySrv, 'success');

                addCtrl.saveBook({});
                addScope.$digest();

                expect(bookMocks.bookRepository.createBook).toHaveBeenCalled();
                expect(bookMocks.addState.go).toHaveBeenCalled();
                expect(bookMocks.addState.go).toHaveBeenCalledWith('^.List');
                expect(bookMocks.notifySrv.success).toHaveBeenCalled();
                expect(bookMocks.notifySrv.success).toHaveBeenCalledWith('books.successPosted');
            });
        });

        describe('edit book', function () {
            it('should throw error message', function () {
                spyOn(bookMocks.bookRepository, 'updateBook').and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.reject({
                        data: {
                            message: 'Error happened'
                        }
                    });
                    return deferred.promise;
                });
                spyOn(bookMocks.notifySrv, 'error');

                editCtrl.saveBook({});
                editScope.$digest();

                expect(bookMocks.bookRepository.updateBook).toHaveBeenCalled();
                expect(bookMocks.notifySrv.error).toHaveBeenCalled();
                expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('Error happened');
            });

            it('should update book and display succesful message', function () {
                spyOn(bookMocks.bookRepository, 'updateBook').and.callFake(function () {
                    var deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
                spyOn(bookMocks.notifySrv, 'success');

                editCtrl.saveBook({});
                editScope.$digest();

                expect(bookMocks.bookRepository.updateBook).toHaveBeenCalled();
                expect(bookMocks.notifySrv.success).toHaveBeenCalled();
                expect(bookMocks.notifySrv.success).toHaveBeenCalledWith('books.successUpdated');
            });
        });
    });

    describe('returnbook', function () {
        it('should throw error message', function () {
            spyOn(bookMocks.bookRepository, 'returnBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({
                    data: {
                        message: 'Error happened'
                    }
                });
                return deferred.promise;
            });
            spyOn(bookMocks.notifySrv, 'error');

            editCtrl.returnBook({bookOfficeId: 10});
            editScope.$digest();

            expect(bookMocks.bookRepository.returnBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.returnBook).toHaveBeenCalledWith(10);
            expect(bookMocks.notifySrv.error).toHaveBeenCalled();
            expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('Error happened');
        });

        it('should take book and display succesful message', function () {
            spyOn(bookMocks.bookRepository, 'returnBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });
            spyOn(bookMocks.notifySrv, 'success');

            editCtrl.returnBook({bookOfficeId: 10});
            editScope.$digest();

            expect(bookMocks.bookRepository.returnBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.returnBook).toHaveBeenCalledWith(10);
            expect(bookMocks.notifySrv.success).toHaveBeenCalled();
            expect(bookMocks.notifySrv.success).toHaveBeenCalledWith('books.successReturned');
        });
    });

    describe('takebook', function () {
        it('should throw error message', function () {
            spyOn(bookMocks.bookRepository, 'takeBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({
                    data: {
                        message: 'Error happened'
                    }
                });
                return deferred.promise;
            });
            spyOn(bookMocks.notifySrv, 'error');

            editCtrl.takeBook({bookOfficeId: 10});
            editScope.$digest();

            expect(bookMocks.bookRepository.takeBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.takeBook).toHaveBeenCalledWith(10);
            expect(bookMocks.notifySrv.error).toHaveBeenCalled();
            expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('Error happened');
        });

        it('should take book and display succesful message', function () {
            spyOn(bookMocks.bookRepository, 'takeBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });
            spyOn(bookMocks.notifySrv, 'success');

            editCtrl.takeBook({bookOfficeId: 10});
            editScope.$digest();

            expect(bookMocks.bookRepository.takeBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.takeBook).toHaveBeenCalledWith(10);
            expect(bookMocks.notifySrv.success).toHaveBeenCalled();
            expect(bookMocks.notifySrv.success).toHaveBeenCalledWith('books.successTaken');
        });
    });

    describe('deletebook', function () {
        it('should throw error message', function () {
            spyOn(bookMocks.bookRepository, 'deleteBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({
                    data: {
                        message: 'Error happened'
                    }
                });
                return deferred.promise;
            });
            spyOn(bookMocks.notifySrv, 'error');

            editCtrl.deleteBook(bookMocks.book);
            editScope.$digest();

            expect(bookMocks.bookRepository.deleteBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.deleteBook).toHaveBeenCalledWith(bookMocks.book.id);
            expect(bookMocks.notifySrv.error).toHaveBeenCalled();
            expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('Error happened');
        });

        it('should delete book and display succesful message', function () {
            spyOn(bookMocks.bookRepository, 'deleteBook').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
            });
            spyOn(bookMocks.editState, 'go');
            spyOn(bookMocks.notifySrv, 'success');

            var message = String.format('books.successDeleted', bookMocks.book.title, bookMocks.book.author);
            editCtrl.deleteBook(bookMocks.book);
            editScope.$digest();

            expect(bookMocks.bookRepository.deleteBook).toHaveBeenCalled();
            expect(bookMocks.bookRepository.deleteBook).toHaveBeenCalledWith(bookMocks.book.id);
            expect(bookMocks.editState.go).toHaveBeenCalled();
            expect(bookMocks.editState.go).toHaveBeenCalledWith('^.List');
            expect(bookMocks.notifySrv.success).toHaveBeenCalled();
            expect(bookMocks.notifySrv.success).toHaveBeenCalledWith(message);
        });
    });

    describe('findBookByIsbn', function () {
        it('should throw error that book cannot be found on error code 100', function () {
            spyOn(bookMocks.bookRepository, 'findBookByIsbn').and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({
                    data: {
                        message: '100'
                    }
                });
                return deferred.promise;
            });
            spyOn(bookMocks.notifySrv, 'error');

            addCtrl.findBookByIsbn();
            addScope.$digest();

            expect(bookMocks.notifySrv.error).toHaveBeenCalled();
            expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('books.notFoundFromExternalProvider');
        });

        it('should throw error that book already exists on error code 101', function () {
            spyOn(bookMocks.bookRepository, 'findBookByIsbn').and.callFake(function () {
                var deferred = $q.defer();
                deferred.reject({
                    data: {
                        message: '101'
                    }
                });
                return deferred.promise;
            });
            spyOn(bookMocks.notifySrv, 'error');

            addCtrl.findBookByIsbn();
            addScope.$digest();

            expect(bookMocks.notifySrv.error).toHaveBeenCalled();
            expect(bookMocks.notifySrv.error).toHaveBeenCalledWith('books.bookAlreadyExists');
        });

        it('should find book and set book title, author and url', function () {
            var book = bookMocks.book;
            spyOn(bookMocks.bookRepository, 'findBookByIsbn').and.callFake(function () {
                var deferred = $q.defer();
                deferred.resolve(book);
                return deferred.promise;
            });

            addCtrl.findBookByIsbn();
            addScope.$digest();

            expect(addCtrl.book.url).toEqual(book.url);
            expect(addCtrl.book.title).toEqual(book.title);
            expect(addCtrl.book.author).toEqual(book.author);
        });
    });

    describe('normalizeUrl', function () {
        it('should add "http://" if there is no such string at start', function () {
            var url = addCtrl.normalizeUrl('www.homepage.com');

            expect(url).toEqual('http://www.homepage.com');
        });

        it('should return string if there is "http://" at start', function () {
            var url = addCtrl.normalizeUrl('http://www.homepage.com');

            expect(url).toEqual('http://www.homepage.com');
        });
    });
});
