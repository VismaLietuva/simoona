var photoCropUploadMocks = {};

beforeEach(function () {
    var testBlob = new Blob([''], {
        type: 'image/png'
    });
    testBlob['lastModifiedDate'] = 1461679475449;
    testBlob['name'] = 'test.png';

    photoCropUploadMocks = {
        toaster: {
            notifySrv: function() {}
        },
        $translate: {
            notifySrv: function() {}
        },
        imageValidationSettings: {},
        shroomsFileUploader: {
            validate: function () {
                return true;
            }
        },
        defaultDirective: '<ace-photo-crop-upload image="{image:0}" is-crop-visible="true" aspect-ratio="1.7" result-image="{resultImage:0}" image-size="{w: 100, h: 100}"></ace-photo-crop-upload>',
        testBlob: testBlob
    }
});
