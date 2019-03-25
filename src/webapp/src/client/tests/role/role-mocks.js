var roleMocks = {};

beforeEach(function () {
    roleMocks = {
        authService: {
            hasPermissions: function () {
                return true;
            }
        },
        $resource: function () { },
        notifySrv: {
            error: function () { },
            success: function () { }
        },
        endpoint: '',
        roles: {
            pageCount: 1,
            pagedList: [
                { id: 0, name: 'Admin' }
            ]
        },
        $advancedLocation: {
            search: function () { }
        },
        localeSrv: {
            formatTranslation: function () { }
        },
        role: {
            id: 0,
            name: 'Admin',
            permissions: [
                {
                    name: 'administration',
                    activeScope: 'admin'
                }
            ]
        },
        errorHandler: {
            handleErrorMessage: function () { },
            handleError: function () { }
        },
        $state: {
            current: {
                name: ''
            },
            params: {
                type: ''
            },
            includes: function (stateName) { },
            go: function () { }
        },
        controllers: [],
        roleCreateState: 'Root.WithOrg.Admin.Roles.Create',
        roleEditState: 'Root.WithOrg.Admin.Roles.Edit'
    };
});