var organizationalStructureMocks = {};

beforeEach(function () {
    organizationalStructureMocks = {
        $windowProvider: {
            $get: function() {
                return {
                    isPremium: false
                }
            }
        },
        lodash: {},
        $translate: {},
        orgTreeData: {
            'fullName': 'One',
            'pictureId': 'a.jpg',
            'children': [{
                'fullName': 'Two One',
                'pictureId': 'a.jpg',
                'children': []
            }, {
                'fullName': 'Two Two',
                'pictureId': 'a.jpg',
                'children': []
            }, {
                'fullName': 'Two Three',
                'pictureId': 'a.jpg',
                'children': [{
                    'fullName': 'Three One',
                    'pictureId': 'a.jpg',
                    'children': [{
                        'fullName': 'Four One',
                        'pictureId': 'a.jpg',
                        'children': [{
                            'fullName': 'Five One',
                            'pictureId': 'a.jpg',
                            'children': []
                        }, {
                            'fullName': 'Five Two',
                            'pictureId': 'a.jpg',
                            'children': []
                        }]
                    }]
                }]
            }]
        },
        resource: {
            organizationalStructure: {

            }
        },
        expectedResult: 'expected result'
    };
});
