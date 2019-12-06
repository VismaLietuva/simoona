(function () {
    'use strict';

    angular
        .module('simoonaApp.Books')
        .directive('aceBooksReportModal', booksReportModal)
        .constant('reports', [
            {name: 'Did not find this book', id: 1},
            {name: 'Lost this book', id: 2},
            {name: 'Other', id: 3},
        ])
        booksReportModal.$inject = [
        '$uibModal'
    ];

    function booksReportModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceBooksReportModal: '=?'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, elem) {
            elem.bind('click', function () {
                $uibModal.open({
                    templateUrl: 'app/books/books-report-modal.html',
                    controller: booksReportController,
                    controllerAs: 'vm',
                    resolve: {
                        currentBook: function () {
                            return scope.$parent.vm.book;
                        }
                    }
                });
            });
        }
    }

    booksReportController.$inject = [
        '$scope',
        '$uibModalInstance',
        'notifySrv',
        'localeSrv',
        'errorHandler',
        '$window',
        'currentBook',
        'reports'
    ];

    function booksReportController($scope, $uibModalInstance, notifySrv, localeSrv, errorHandler, $window, currentBook, reports) {
        var vm = this;
        
        vm.book = currentBook;
        vm.reports = reports
        
        vm.report = report;
        vm.cancel = cancel;

        function init() {

        }

        
        function report(book) {
            
             console.log(vm.selectedReport);
            bookRepository.reportBook(book.bookOfficeId).then(function () {
                var message = localeSrv.formatTranslation('books.successReported', {one: book.title, two: book.author});
                notifySrv.success(message);
            }, function (response) {
                notifySrv.error(response.data.message);
            });
        }

        function cancel() {
            $uibModalInstance.dismiss('cancel');
        }
    }
})();
