var wallMocks = {};

﻿beforeEach(function() {
    wallMocks = {
        authService: { identity: { userId: 1 }, hasPermissions: function() { return true; } },
    };
});
