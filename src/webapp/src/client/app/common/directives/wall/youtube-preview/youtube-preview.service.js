(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('youtubePreviewService', youtubePreviewService);

    youtubePreviewService.$inject = ['$resource'];

    function youtubePreviewService($resource) {
        /*jshint camelcase: false */
        return $resource('https://www.googleapis.com/youtube/v3/videos?callback=JSON_CALLBACK&prettyprint=true', {}, {
            jsonp_query: {
                method: 'JSONP',
                params: {
                    id: '@id',
                    key: 'AIzaSyDUUT_Y63Z_E6YrQQB_5czzHj0139cXkbk',
                    part: 'snippet',
                    fields: 'items(snippet(title))'
                },
                bypassErrors: [404]
            }
        });
    }
}());
