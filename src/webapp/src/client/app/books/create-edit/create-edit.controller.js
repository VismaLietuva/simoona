(function () {
    'use strict';

    angular
        .module('simoonaApp.Books')
        .constant('bookSettings', {
            bookISBNLength: 20,
            bookQuantityMinLength: 1
        })
        .controller('booksCreateEditController', booksCreateEditController);

    booksCreateEditController.$inject = [
        '$rootScope',
        '$scope',
        '$state',
        '$stateParams',
        'notifySrv',
        'authService',
        'bookRepository',
        'bookCreateEditService',
        'bookSettings',
        'localeSrv'
    ];

    function booksCreateEditController($rootScope, $scope, $state, $stateParams,
        notifySrv, authService, bookRepository, bookCreateEditService, bookSettings, localeSrv) {
        /*jshint validthis: true */
        var vm = this;

        vm.book = {};
        vm.owner = {
            id: null,
            fullName: null
        };
        vm.offices = {};

        vm.identity = authService.identity;
        vm.isBookAdministrator = authService.hasPermissions(['BOOK_ADMINISTRATION']);

        vm.state = {
            add: $state.includes('Root.WithOrg.Client.Books.Add'),
            edit: $state.includes('Root.WithOrg.Client.Books.Edit')
        };

        $rootScope.pageTitle = vm.state.add ? 'books.addBookTitle' : 'books.editBookTitle';

        vm.isbnInvalid = localeSrv.formatTranslation('common.messageInvalidLength', {one: bookSettings.bookISBNLength});

        vm.searchUsers = searchUsers;
        vm.saveBook = saveBook;
        vm.takeBook = takeBook;
        vm.returnBook = returnBook;
        vm.returnBookAdmin = returnBookAdmin;
        vm.deleteBook = deleteBook;
        vm.normalizeUrl = normalizeUrl;
        vm.getBookDetails = getBookDetails;
        vm.findBookByIsbn = findBookByIsbn;

        init();

        ////////

        function init() {
            vm.isActionColumn = false;

            bookRepository.getAllOffices().then(function (response) {
                vm.offices = response;

                if (vm.state.add) {
                    vm.book.quantityByOffice = bookCreateEditService.getOfficeQuantities(vm.offices, null);
                }

                if (vm.state.edit) {
                    vm.currentOffice = bookCreateEditService.getCurrentOffice(vm.offices, $stateParams.officeId);

                    getBookDetails();
                }
            });

            $scope.$watch('vm.owner', function(newVal) {
                if (newVal && !newVal.id) {
                    vm.ownerError = true;
                } else {
                    vm.ownerError = null;
                }
            }, true);
        }

        function getBookDetails() {
            if (!vm.isBookAdministrator) {
                bookRepository.getBookDetails($stateParams.id).then(function (response) {
                    vm.book = response;

                    vm.isActionColumn = bookCreateEditService.getActionColumn(vm.book.bookLogs, vm.identity);
                });
            } else {
                bookRepository.getBookDetailsForAdministrator($stateParams.id).then(function (response) {
                    vm.book = response.bookDetails;
                    vm.owner.id = vm.book.ownerId;
                    vm.owner.fullName = vm.book.ownerFullName;

                    vm.book.quantityByOffice = bookCreateEditService.getOfficeQuantities(vm.offices, response.quantityByOffice);

                    vm.isActionColumn = bookCreateEditService.getActionColumn(vm.book.bookLogs, vm.identity);
                });
            }
        }

        function saveBook(book) {
            vm.book.ownerId = vm.owner.id;
            if (vm.state.add) {
                vm.book.offices = bookCreateEditService.filterOffices(vm.book);

                bookRepository.createBook(book).then(function () {
                    var message = localeSrv.formatTranslation('books.successPosted', {one: book.title, two: book.author});
                    $state.go('^.List');
                    notifySrv.success(message);
                }, function (response) {
                    if (response.data.message === '101') {
                        notifySrv.error('books.bookAlreadyExists');
                    } else if (response.data.message === '102') {
                        notifySrv.error('books.bookAllQuantitiesAreZero');
                    }
                    else {
                        notifySrv.error(response.data.message);
                    }
                });
            }

            if (vm.state.edit) {
                bookRepository.updateBook(book).then(function () {
                    var message = localeSrv.formatTranslation('books.successUpdated', {one: book.title, two: book.author});
                    getBookDetails();
                    notifySrv.success(message);
                }, function (response) {
                    if (response.data.message === '102') {
                        notifySrv.error('books.bookAllQuantitiesAreZero');
                    }
                    else {
                        notifySrv.error(response.data.message);
                    }
                });
            }
        }

        function returnBook(book) {
            bookRepository.returnBook(book.bookOfficeId).then(function () {
                var message = localeSrv.formatTranslation('books.successReturned', {one: book.title, two: book.author});
                getBookDetails();
                notifySrv.success(message);
            }, function (response) {
                notifySrv.error(response.data.message);
            });
        }

        function returnBookAdmin(book, userId) {
            bookRepository.returnBookAdmin(book.bookOfficeId, userId).then(function () {
                var message = localeSrv.formatTranslation('books.successReturned', {one: book.title, two: book.author});
                getBookDetails();
                notifySrv.success(message);
            }, function (response) {
                notifySrv.error(response.data.message);
            });
        }

        function takeBook(book) {
            bookRepository.takeBook(book.bookOfficeId).then(function () {
                var message = localeSrv.formatTranslation('books.successTaken', {one: book.title, two: book.author});
                getBookDetails();
                notifySrv.success(message);
            }, function (response) {
                notifySrv.error(response.data.message);
            });
        }

        function deleteBook(book) {
            bookRepository.deleteBook(book.id).then(function () {
                var message = localeSrv.formatTranslation('books.successDeleted', {one: book.title, two: book.author});
                $state.go('^.List');
                notifySrv.success(message);
            }, function (response) {
                notifySrv.error(response.data.message);
            });
        }

        function findBookByIsbn(isbn) {
            bookRepository.findBookByIsbn(isbn).then(function (response) {
                vm.book.url = response.url;
                vm.book.title = response.title;
                vm.book.author = response.author;
            }, function (response) {
                if (response.data.message === '100') {
                    notifySrv.error('books.notFoundFromExternalProvider');
                } else if (response.data.message === '101') {
                    notifySrv.error('books.bookAlreadyExists');
                } else {
                    notifySrv.error(response.data.message);
                }
            });
        }

        function normalizeUrl(url) {
            if (url.indexOf('http://') === -1 && url.indexOf('https://') === -1) {
                return 'http://' + url;
            }

            return url;
        }

        function searchUsers(search) {
            return bookRepository.getUserForAutoCompleteResponsiblePerson(search);
        }
    }
})();
