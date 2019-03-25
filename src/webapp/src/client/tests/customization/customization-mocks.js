var customizationMocks = {};

beforeEach(function() {
    customizationMocks = {
        $resource: {},
        endPoint: '',
        lodash: {
            filter: function() {}
        },
        $state: {
            go: function() {}
        },
        externalLinksRepository: {
            getExternalLinks: function() {},
            postExternalLinks: function() {}
        },
        notifySrv: {
            error: function() {},
            success: function() {}
        },
        errorHandler: {
            handleErrorMessage: function() {}
        },
        externalLinks: [
            { 'id': 181, 'name': 'Testco', 'url': 'http://www.test.com' },
            { 'id': 183, 'name': 'Delfi', 'url': 'https://www.delfi.lt' },
            { 'id': 184, 'name': 'Goda', 'url': 'http://god.lt/' }
        ],
        externalLinksAfterDelete: [
            { 'id': 181, 'name': 'Testco', 'url': 'http://www.test.com' },
            { 'id': 184, 'name': 'Goda', 'url': 'http://god.lt/' }
        ],
        externalLinksWithAddedLink: [
            { 'id': 181, 'name': 'Testco', 'url': 'http://www.test.com' },
            { 'id': 183, 'name': 'Delfi', 'url': 'https://www.delfi.lt' },
            { 'id': 184, 'name': 'Goda', 'url': 'http://god.lt/' },
            { name: '', url: '' }
        ],
        externalLinksUpdateObject: {
            LinksToUpdate: [],
            LinksToCreate: [],
            LinksToDelete: []
        }
    };
});
