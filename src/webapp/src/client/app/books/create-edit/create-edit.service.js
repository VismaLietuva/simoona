(function() {
    'use strict';

    angular
        .module('simoonaApp.Books')
        .service('bookCreateEditService', bookCreateEditService);

    bookCreateEditService.$inject = [
        'lodash'
    ];

    function bookCreateEditService(lodash) {
        var service = {
            filterOffices: filterOffices,
            getActionColumn: getActionColumn,
            getCurrentOffice: getCurrentOffice,
            getOfficeQuantities: getOfficeQuantities
        };
        return service;

        /////

        function getOfficeQuantities(offices, quantities) {
            var quantityByOffice = [];

            lodash.each(offices, function (office) {
                var bookQuantity = 0;

                if (quantities) {
                    var quantityOffice = lodash.find(quantities, function (quantity) {
                        return quantity.officeId === office.id;
                    });

                    if (quantityOffice) {
                        bookQuantity = quantityOffice.bookQuantity;
                    }
                }

                quantityByOffice.push({
                    officeId: office.id,
                    officeName: office.name,
                    bookQuantity: bookQuantity
                });
            });

            return quantityByOffice;
        }

        function getCurrentOffice(offices, officeIdString) {
            var officeId = parseInt(officeIdString);

            return lodash.find(offices, function (office) {
                return office.id === officeId;
            });
        }

        function getActionColumn(bookLogs, identity) {
            return lodash.some(bookLogs, function (bookLog) {
                return bookLog.userId === identity.userId && !bookLog.returned;
            });
        }

        function filterOffices(book) {
            return lodash.filter(book.quantityByOffice, function (office) {
                return office.bookQuantity;
            });
        }
    }
})();
