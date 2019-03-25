(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('pictureRepository', pictureRepository);

    pictureRepository.$inject = [
        '$resource',
        'shroomsFileUploader',
        'endPoint'
    ];

    function pictureRepository($resource, shroomsFileUploader, endPoint) {
        var url = endPoint + '/Picture/';

        var service = {
            delete: deleteImage,
            upload: uploadImage
        };
        return service;

        //////

        function deleteImage(pictureId) {
            return $resource(url + 'Delete').delete({
                id: pictureId
            }).$promise;
        }

        function uploadImage (files) {
            return shroomsFileUploader.upload(files, url + 'Upload');
        }
    }
})();
