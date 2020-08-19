(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('mentionService', mentionService);

    mentionService.$inject = ['wallCommentRepository'];

    function mentionService(wallCommentRepository) {
        var service = {
            getUsersForAutocomplete: getUsersForAutocomplete,
            compareAndGetMentions : compareAndGetMentions
        };

        return service;

        function getUsersForAutocomplete(query) {
            return wallCommentRepository.getUsersForAutoComplete(query);
        }

        function compareAndGetMentions(messageBody, selectedMentions) {
            var parsedNamesFromTextBody = parseMentions(messageBody);

            if (parsedNamesFromTextBody) {
                return selectedMentions.filter(function(cur) {
                    if(parsedNamesFromTextBody.includes(cur.fullName)) {
                        return cur;
                    }
                }).map(function(cur) {
                    return cur.id;
                });
            }
        }
        
        function parseMentions (text) {
            var pattern = /\B@[a-z0-9_-]+/gi;
            var matches = text.match(pattern);
            
            if (matches) {
                return matches.map(cur => {
                    return cur.replace('@', '')
                             .replace('_', ' ');
                });
            }
        }
    }
})();