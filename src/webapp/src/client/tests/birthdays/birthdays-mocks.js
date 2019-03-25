var birthdaysMocks = {};

beforeEach(function () {
    birthdaysMocks = {
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
        birthdaysRepository: {
            getUsers: function () { }
        }
    }
});
