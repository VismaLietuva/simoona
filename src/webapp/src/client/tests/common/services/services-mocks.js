var servicesMocks = {};

beforeEach(function () {
    servicesMocks = {
        lodash: {},
        localeSrv: {
            formatTranslation: function(key) { return key; }
        },
        notifySrv: {
            error: function () {},
            success: function () {}
        },
        errorCodeMessages: {
            200: 'invalidType',
            201: ['errorMessage', 'invalidType']
        }
    };
});
