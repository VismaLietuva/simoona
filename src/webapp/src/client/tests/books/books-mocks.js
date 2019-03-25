var bookMocks = {};

beforeEach(function () {
    bookMocks = {
        localeSrv: {
            formatTranslation: function(key) { return key; }
        },
        $resource: {},
        endPoint: '',
        $translate: {
            instant: function () {}
        },
        bookSettings: {
            fieldValueTooShort: ''
        },
        notifySrv: {
            error: function () {},
            success: function () {}
        },
        userAuthService: {
            hasPermissions: function () {
                return false;
            }
        },
        authService: {
            hasPermissions: function () {
                return true;
            }
        },
        addState: {
            go: function () {},
            includes: function (state) {
                if (state === 'Root.WithOrg.Client.Books.Add') {
                    return true;
                }

                return false;
            }
        },
        editState: {
            go: function () {},
            includes: function (state) {
                if (state === 'Root.WithOrg.Client.Books.Edit') {
                    return true;
                }

                return false;
            }
        },
        bookRepository: {
            takeBook: function () {},
            createBook: function () {},
            updateBook: function () {},
            returnBook: function () {},
            deleteBook: function () {},
            getAllOffices: function () {},
            getFilteredBooks: function () {},
            findBookByIsbn: function () {},
            getBookDetails: function () {},
            getBookDetailsForAdministrator: function () {}
        },
        bookCreateEditService: {
            filterOffices: function () {},
            getActionColumn: function () {},
            getCurrentOffice: function () {
                return 'Vilnius';
            },
            getOfficeQuantities: function () {
                return [
                    {id: 1, title: 'Vilnius', quantity: 1},
                    {id: 2, title: 'Kaunas', quantity: 0}
                ];
            }
        },
        bookInfo: {
            bookDetails: {id: 0, title: 'C# book', author: 'Jim Steward', url: 'http://google.lt/'}
        },
        bookInfoForAdmin: {
            bookDetails: {id: 0, title: 'C# book', author: 'Jim Steward', url: 'http://google.lt/'},
            quantityByOffice: {officeId: 1, officeName: 'Vilnius', bookQuantity: 2}
        },
        book: {id: 0, title: 'C# book', author: 'Jim Steward', url: 'http://google.lt/'},
        bookQuantityOffices: {
            quantityByOffice: [
                {bookQuantity: 10},
                {bookQuantity: 5},
                {bookQuantity: 0}
            ],
        },
        books1: {
            page: 1,
            pageSize: 10,
            itemCount: 20,
            pageCount: 2,
            entries: [
                {id: 0, title: 'C# book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 1, title: 'PHP book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 2, title: 'Js book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 3, title: 'HTML book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 4, title: 'Css book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 5, title: 'JsHint book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 6, title: 'Angular book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 7, title: 'TypeScript book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 8, title: 'React book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 9, title: 'Java book', author: 'Jim Steward', url: 'http://google.lt/'},
            ]
        },
        books2: {
            page: 2,
            pageSize: 10,
            itemCount: 20,
            pageCount: 2,
            entries: [
                {id: 10, title: 'A++ book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 11, title: 'Algol book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 12, title: 'Assembler book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 13, title: 'Bash book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 14, title: 'C book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 15, title: 'Python book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 16, title: 'Claire book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 17, title: 'Clojure book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 18, title: 'FoxPro book', author: 'Jim Steward', url: 'http://google.lt/'},
                {id: 19, title: 'Go book', author: 'Jim Steward', url: 'http://google.lt/'},
            ]
        },
        offices: [
            {id: 1, title: 'Vilnius'},
            {id: 2, title: 'Kaunas'}
        ],
        officeQuantities: [
            {id: 1, name: 'Vilnius'},
            {id: 2, name: 'Kaunas'}
        ],
        officeQuanititesNoBooksExpectedResult: [
            {officeId: 1, officeName: 'Vilnius', bookQuantity: 0},
            {officeId: 2, officeName: 'Kaunas', bookQuantity: 0}
        ],
        officeQuanititesWithBooksExpectedResult: [
            {officeId: 1, officeName: 'Vilnius', bookQuantity: 5},
            {officeId: 2, officeName: 'Kaunas', bookQuantity: 10}
        ]
    };
});
