describe('acePhotoCropUpload', function() {
    var element, scope, ctrl;

    beforeEach(module('simoonaApp.Common'));
    beforeEach(function() {
        module(function($provide) {
            $provide.value('shroomsFileUploader', photoCropUploadMocks.shroomsFileUploader);
            $provide.value('imageValidationSettings', photoCropUploadMocks.imageValidationSettings);
            $provide.value('toaster', photoCropUploadMocks.toaster);
            $provide.value('$translate', photoCropUploadMocks.$translate);
        });
    });

    describe('photoCropUpload', function() {
        it('should be initialized', function() {
            inject(function($compile, $rootScope, $templateCache) {
                scope = $rootScope.$new();

                $templateCache.put('app/common/directives/photo-crop-upload/photo-crop-upload.html', '<div></div>');
                element = angular.element(photoCropUploadMocks.defaultDirective);

                $compile(element)(scope);
                scope.$digest();

                ctrl = element.controller('acePhotoCropUpload');
            })

            expect(ctrl).toBeDefined();

            expect(ctrl.image).toBeDefined();
            expect(ctrl.isCropVisible).toBeDefined();
            expect(ctrl.aspectRatio).toBeDefined();
            expect(ctrl.resultImage).toBeDefined();
            expect(ctrl.imageSize).toBeDefined();
        });

        it('should set image after calling handleFileSelect with file selected event', function() {
            scope.$$childTail.handleFileSelect([photoCropUploadMocks.testBlob]);
            expect(ctrl.image).toEqual(photoCropUploadMocks.testBlob);
        });
    });
});
