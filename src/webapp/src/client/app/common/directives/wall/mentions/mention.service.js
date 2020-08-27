(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('mentionService', mentionService);

    mentionService.$inject = [
        'authService',
        'wallCommentRepository'
    ];

    function mentionService(authService, wallCommentRepository) {
        var service = {
            getUsersForAutocomplete: getUsersForAutocomplete,
            applyMentions: applyMentions
        };

        return service;

        function applyMentions(form, selectedMentionsFromList) {
            form.mentionedUserIds = [];

            var parsedNamesFromTextBody = parseMentions(form.messageBody);

            parsedNamesFromTextBody = removeDuplicates(parsedNamesFromTextBody);
            selectedMentionsFromList = removeDuplicates(selectedMentionsFromList, 'fullName');

            if (parsedNamesFromTextBody && selectedMentionsFromList) {
                selectedMentionsFromList.forEach(function(cur) {
                    if(parsedNamesFromTextBody.includes(cur.fullName)) {
                        form.mentionedUserIds.push(cur.id);
                    }
                });
            }
        }

        function getUsersForAutocomplete(query) {
            return wallCommentRepository.getUsersForAutoComplete(query);
        }

        function parseMentions (text) {
            var pattern = /\B@[\u00BF-\u1FFF\u2C00-\uD7FF\w]+/gi;
            var matches = text.match(pattern);

            if (matches) {
                matches = removeDuplicates(matches);

                return matches.map(cur => {
                    return cur.replace('@', '')
                             .replace('_', ' ');
                });
            }
        }

        function removeDuplicates(myArr, prop) {
            if (myArr) {
                return myArr.filter((obj, pos, arr) => {
                    return prop ? 
                        arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos : 
                        arr.map(mapObj => mapObj).indexOf(obj) === pos;
                });
            }
        }
    }
})();