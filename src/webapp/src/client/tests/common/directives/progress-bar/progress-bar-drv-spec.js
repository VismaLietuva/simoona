describe('aceProgressBar', function() {
    var element, scope, ctrl;

    beforeEach(module('simoonaApp.Common'));

    describe('progressBar', function() {
        it('should be initialized', function() {
            inject(function($compile, $rootScope, $templateCache) {
                scope = $rootScope.$new();

                $templateCache.put('app/common/directives/progress-bar/progress-bar.html', '<div></div>');
                element = angular.element(progressBarMocks.defaultDirective);

                $compile(element)(scope);
                scope.$digest();

                ctrl = element.controller('aceProgressBar');
            });

            expect(ctrl).toBeDefined();

            expect(ctrl.value).toBeDefined();
            expect(ctrl.max).toBeDefined();
        });

        it('should set isFull true if value and max are equal and fullDanger is set to true', function() {
            inject(function($compile, $rootScope, $templateCache) {
                scope = $rootScope.$new();

                $templateCache.put('app/common/directives/progress-bar/progress-bar.html', '<div></div>');
                element = angular.element(progressBarMocks.fullTrueDirective);

                $compile(element)(scope);
                scope.$digest();

                ctrl = element.controller('aceProgressBar');
            });

            expect(ctrl.percents).toEqual('100.00');
            expect(ctrl.isFull).toBeTruthy();
        });

        it('should set isFull false if value and max are equal and fullDanger is not set', function() {
            inject(function($compile, $rootScope, $templateCache) {
                scope = $rootScope.$new();

                $templateCache.put('app/common/directives/progress-bar/progress-bar.html', '<div></div>');
                element = angular.element(progressBarMocks.fullUndefinedDirective);

                $compile(element)(scope);
                scope.$digest();

                ctrl = element.controller('aceProgressBar');
            });

            expect(ctrl.percents).toEqual('100.00');
            expect(ctrl.isFull).toBeFalsy();
        });

        it('should set percents to 50.00% if value is 5 and max is 10', function() {
            inject(function($compile, $rootScope, $templateCache) {
                scope = $rootScope.$new();

                $templateCache.put('app/common/directives/progress-bar/progress-bar.html', '<div></div>');
                element = angular.element(progressBarMocks.halfUndefinedDirective);

                $compile(element)(scope);
                scope.$digest();

                ctrl = element.controller('aceProgressBar');
            });

            expect(ctrl.percents).toEqual('50.00');
            expect(ctrl.isFull).toBeFalsy();
        });

        it('should set percents to 50.00% and isFull set false if value is 5, max is 10 and fullDanger is true', function() {
            inject(function($compile, $rootScope, $templateCache) {
                scope = $rootScope.$new();

                $templateCache.put('app/common/directives/progress-bar/progress-bar.html', '<div></div>');
                element = angular.element(progressBarMocks.halfTrueDirective);

                $compile(element)(scope);
                scope.$digest();

                ctrl = element.controller('aceProgressBar');
            });

            expect(ctrl.percents).toEqual('50.00');
            expect(ctrl.isFull).toBeFalsy();
        });

    });
});
