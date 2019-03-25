(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('dataHandler', dataHandler);

    dataHandler.$inject = [];

    function dataHandler() {
        var service = {
            dataURItoBlob: dataURItoBlob
        };
        return service;

        /////////

        function dataURItoBlob(dataURI, type) {
            // convert base64/URLEncoded data component to raw binary data held in a string
            var byteString;
            if (dataURI.split(',')[0].indexOf('base64') >= 0) {
                byteString = atob(dataURI.split(',')[1]);
            } else {
                byteString = unescape(dataURI.split(',')[1]);
            }

            // write the bytes of the string to a typed array
            var intArray = new Uint8Array(byteString.length);
            for (var i = 0; i < byteString.length; i++) {
                intArray[i] = byteString.charCodeAt(i);
            }

            return new Blob([intArray], {
                type: type
            });
        }
    }

})();
