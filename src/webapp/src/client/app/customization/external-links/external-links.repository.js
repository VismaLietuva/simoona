(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.ExternalLinks')
        .factory('externalLinksRepository', externalLinksRepository);

    externalLinksRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function externalLinksRepository($resource, endPoint) {
        var externalLinkUrl = endPoint + '/ExternalLink/';

        var service = {
            getExternalLinks: getExternalLinks,
            postExternalLinks: postExternalLinks
        };
        return service;

        /////////

        function getExternalLinks() {
            return $resource(externalLinkUrl + 'List').query().$promise;
        }

        function postExternalLinks(object) {
            return $resource(externalLinkUrl + 'Update').save(object).$promise;
        }

    }
})();
