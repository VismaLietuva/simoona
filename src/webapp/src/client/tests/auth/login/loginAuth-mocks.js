var loginAuthMocks = {};

beforeEach(function () {
    loginAuthMocks = {
        localStorageService: {
            get: function () { }
        },
        $resource: {},
        appConfig: {
            homeStateName: 'Root.WithOrg.Client.Wall.Item.Feed',
            adminRole: 'Admin',
            allowedStatesForNewUser: ['Root.WithOrg.Client.Profiles.Edit', 'Root.WithOrg.LogOff'],
            defaultOrganization: 'Visma',
            lodash: {}
        },
        localeSrv: {
            formatTranslation: function (key) { return key; }
        },
        endPoint: '',
        notifySrv: {
            error: function () { },
            success: function () { }
        },
        errorHandler: {
            handleErrorMessage: function () { }
        },
        authService: {
            hasPermissions: function () {
                return true;
            },
            getOrganizationNameFromUrl: function () {
                return 'Organization';
            },
            getHashArrayFromUrl: function () {
                return [
                    {
                        access_token: 'token'
                    }
                ]
            },
            identity: {
                isAuthenticated: true
            },
            getExternalLogins: function () { },
            redirectToHome: function () { },
            getExternalProvider: function (externalProviders, externalProvider) {
                return externalProviders.includes(externalProvider)
            }
        },
        $timeout: function (success, error) { }
    }
});