describe('aceListFilter', function() {
    var element, scope, ctrl;

    beforeEach(module('simoonaApp.Common'));
    beforeEach(inject(function($compile, $rootScope, $templateCache) {
        scope = $rootScope.$new();

        $templateCache.put('app/common/directives/list-filter/list-filter.html', '<div min-characters="2"></div>');
        element = angular.element('<ace-list-filter></ace-list-filter>');

        $compile(element)(scope);
        scope.$digest();

        ctrl = element.controller('aceListFilter');
    }));

    describe('listFilter', function() {
        beforeEach(function() {
            spyOn(ctrl, 'onFiltering');
        });

        it('should be initialized', function() {
            expect(ctrl).toBeDefined();

            expect(ctrl.onFiltering).toBeDefined();
        });

        it('should filter if length text is 0', function() {
            ctrl.onSearch('');

            expect(ctrl.onFiltering).toHaveBeenCalled();
            expect(ctrl.onFiltering).toHaveBeenCalledWith({search: ''});
        });

        it('should not filter if text length is 1', function() {
            ctrl.onSearch('a');

            expect(ctrl.onFiltering).not.toHaveBeenCalled();
        });

        it('should filter if length is more than 1', function() {
            ctrl.onSearch('ab');

            expect(ctrl.onFiltering).toHaveBeenCalled();
            expect(ctrl.onFiltering).toHaveBeenCalledWith({search: 'ab'});
        });
    });
});
