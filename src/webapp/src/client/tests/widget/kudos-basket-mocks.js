var kudosBasketMocks = {};

beforeEach(function () {
    kudosBasketMocks = {
        appConfig: {
            homeStateName: 'Root.WithOrg.Client.Wall.Item.Feed',
            adminRole: 'Admin',
            allowedStatesForNewUser: ['Root.WithOrg.Client.Profiles.Edit', 'Root.WithOrg.LogOff'],
            defaultOrganization: 'Visma'
        },
        $resource: function () { },
        endPoint: '',
        notifySrv: {
            error: function () { },
            success: function () { }
        },
        kudosBasketRepository: {
            getDonations: function () { },
            makeDonation: function () { },
            getKudosBasket: function () { },
            getKudosBasketWidget: function () { },
            createNewBasket: function () { },
            editKudosBasket: function () { },
            deleteKudosBasket: function () { }
        },
        $state: {
            go: function () { }
        },
        kudosBasketData: {
            id: 1
        }
    };
});
