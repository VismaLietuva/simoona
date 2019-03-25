describe('validIfChanged', function() {
    var scope, form;

    beforeEach(module('simoonaApp.Common'));
    beforeEach(inject(function($compile, $rootScope) {
        scope = $rootScope;

        scope.model = {
            somevalue: null
        };

        var element = angular.element(
            '<form name="form">' +
                '<input ng-model="model.somevalue" name="somevalue" valid-if-changed="oldValue" />' +
            '</form>'
        );

        $compile(element)(scope);
        form = scope.form;
    }));

    describe('integer', function() {
        it('should not pass with same old value', function() {
            form.somevalue.$setViewValue('oldValue');
            scope.$digest();

            expect(scope.model.somevalue).toBeUndefined();
            expect(form.somevalue.$valid).toBe(false);
        });

        it('should pass with new value', function() {
            form.somevalue.$setViewValue('newValue');
            scope.$digest();

            expect(scope.model.somevalue).toEqual('newValue');
            expect(form.somevalue.$valid).toBe(true);
        });
    });
});
