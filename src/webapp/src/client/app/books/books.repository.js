(function() {
    'use strict';

    angular
        .module('simoonaApp.Books')
        .service('bookRepository', bookRepository);

    bookRepository.$inject = [
        '$resource',
        'endPoint',
    ];

    function bookRepository($resource, endPoint) {
        var bookUrl = endPoint + '/book/';
        var officeUrl = endPoint + '/Office/';
        var applicationUrl = endPoint + '/ApplicationUser/';

        var service = {
            takeBook: takeBook,
            returnBook: returnBook,
            returnBookAdmin: returnBookAdmin,
            createBook: createBook,
            updateBook: updateBook,
            deleteBook: deleteBook,
            reportBook: reportBook,
            getAllOffices: getAllOffices,
            findBookByIsbn: findBookByIsbn,
            getBookDetails: getBookDetails,
            getFilteredBooks: getFilteredBooks,
            getBookDetailsForAdministrator: getBookDetailsForAdministrator,
            getUserForAutoCompleteResponsiblePerson: getUserForAutoCompleteResponsiblePerson,
            updateBooksCovers: updateBooksCovers
        };
        return service;

        /////

        function getBookDetails(bookOfficeId) {
            return $resource(bookUrl + 'Details?bookOfficeId=' + bookOfficeId).get().$promise;
        }

        function getBookDetailsForAdministrator(bookOfficeId) {
            return $resource(bookUrl + 'DetailsForAdmin?bookOfficeId=' + bookOfficeId).get().$promise;
        }

        function getFilteredBooks(params) {
            return $resource(bookUrl + 'ListByOffice').get({
                page: params.page,
                officeId: params.officeId,
                searchString: params.search
            }).$promise;
        }

        function createBook(params) {
            return $resource(bookUrl + 'Create', '', {
                post: {
                    method: 'POST'
                }
            }).post(params).$promise;
        }

        function updateBook(params) {
            return $resource(bookUrl + 'Edit', '', {
                put: {
                    method: 'PUT'
                }
            }).put(params).$promise;
        }

        function deleteBook(bookId) {
            return $resource(bookUrl + 'Delete?bookId=' + bookId).delete().$promise;
        }

        function returnBook(bookOfficeId) {
            return $resource(bookUrl + 'Return?bookOfficeId=' + bookOfficeId, '', {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }

        function returnBookAdmin(bookOfficeId, userId) {
            return $resource(bookUrl + 'ReturnForAdmin?bookOfficeId=' + bookOfficeId +'&userId=' + userId, '', {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }

        function reportBook(bookReport) {
            return $resource(bookUrl + 'Report', '', {
                put: {
                    method: 'PUT',
                }
            }).put(bookReport).$promise;
        }

        function takeBook(bookOfficeId) {
            return $resource(bookUrl + 'Take?bookOfficeId=' + bookOfficeId, '', {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }

        function getAllOffices() {
            return $resource(officeUrl + 'GetAllOfficesForDropdown').query().$promise;
        }

        function findBookByIsbn(isbn) {
            return $resource(bookUrl + 'FindByIsbn').get({
                isbn: isbn
            }).$promise;
        }

        function getUserForAutoCompleteResponsiblePerson(params) {
            return $resource(applicationUrl + 'GetForAutoComplete').query({
                s: params
            }).$promise;
        }

        function updateBooksCovers() {
            return $resource(bookUrl + 'covers', '', {
                patch: {
                    method: 'PATCH'
                }
            }).patch().$promise;
        }
    }
})();