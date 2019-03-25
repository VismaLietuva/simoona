describe('aceListPagination', function() {
    var element, scope, ctrl;

    beforeEach(module('simoonaApp.Common'));
    beforeEach(inject(function($compile, $rootScope, $templateCache) {
        scope = $rootScope.$new();

        $templateCache.put('app/common/directives/list-pagination/list-pagination.html', '<div></div>');
        element = angular.element('<ace-list-pagination></ace-list-pagination>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceListPagination');
    }));

    it('controller and methods should be defined', function() {
        expect(ctrl).toBeDefined();

        expect(ctrl.nextPage).toBeDefined();
        expect(ctrl.prevPage).toBeDefined();
        expect(ctrl.lastPage).toBeDefined();
        expect(ctrl.firstPage).toBeDefined();
    });

    describe('test pagination methods', function() {
        beforeEach(function() {
            ctrl.pageCount = 3;
            ctrl.currentPage = 2;

            spyOn(ctrl, 'onChanged');
        });

        it('nextPage should not increase page value if it is already last page', function() {
            ctrl.currentPage = 3;
            ctrl.nextPage();

            expect(ctrl.onChanged).not.toHaveBeenCalled();
        });

        it('nextPage should increase current page value', function() {
            ctrl.nextPage();

            expect(ctrl.onChanged).toHaveBeenCalled();
            expect(ctrl.onChanged).toHaveBeenCalledWith({page: ctrl.currentPage + 1});
        });

        it('prevPage should not decrease current page value if it is already first page', function() {
            ctrl.currentPage = 1;
            ctrl.prevPage();

            expect(ctrl.onChanged).not.toHaveBeenCalled();
        });

        it('prevPage should decrease current page value', function() {
            ctrl.prevPage();

            expect(ctrl.onChanged).toHaveBeenCalled();
            expect(ctrl.onChanged).toHaveBeenCalledWith({page: ctrl.currentPage - 1});
        });

        it('firstPage should set current page value to 1', function() {
            ctrl.firstPage();

            expect(ctrl.onChanged).toHaveBeenCalled();
            expect(ctrl.onChanged).toHaveBeenCalledWith({page: 1});
        });

        it('lastPage should set current page value to pageCount value', function() {
            ctrl.lastPage();

            expect(ctrl.onChanged).toHaveBeenCalled();
            expect(ctrl.onChanged).toHaveBeenCalledWith({page: ctrl.pageCount});
        });
    });
});
