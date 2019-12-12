(function () {
    'use strict';

    angular
        .module('simoonaApp.Books')
        .directive('aceBooksReportModal', booksReportModal)
        booksReportModal.$inject = [
        '$uibModal',
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
        '$translate',
        'bookRepository'
    ];

    function booksReportController($scope, $uibModalInstance, notifySrv, localeSrv, errorHandler, $window, currentBook, $translate, bookRepository) {
        var vm = this;
        vm.book = currentBook;

        vm.reports = [];
        vm.reports.push($translate.instant('books.notFound'));
        vm.reports.push($translate.instant('books.lost'));
        vm.reports.push($translate.instant('books.other'));

        vm.reportBook = reportBook;
        vm.cancel = cancel;
        
        function reportBook() {
            vm.report = {
                bookOfficeId: vm.book.bookOfficeId,
                report: vm.selectedReport, 
                comment: vm.comment
            };
            bookRepository.reportBook(vm.report).then(function () {
                var message = localeSrv.formatTranslation('books.successReported', {one: vm.book.title, two: vm.book.author});
                notifySrv.success(message);
                $uibModalInstance.close();
            }, function (response) {
                notifySrv.error(response.data.message);
            });
        }

        function cancel() {
            $uibModalInstance.dismiss('cancel');
        }
    }
})();
