(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('mentionService', mentionService);

    mentionService.$inject = [ 'wallCommentRepository' ];

    function mentionService(wallCommentRepository) {
        var service = {
            getUsersForAutocomplete: getUsersForAutocomplete,
            mentions: getMentions
        };

        return service;

        function getMentions() {
            return new mentions();
        }

        function mentions() {
            this.selectedMentions = [];
            this.employees = [];

            this.invokeMention = function (term) {
                if (!term) {
                    return;
                }

                getUsersForAutocomplete(term).then((response) => {
                    this.employees = response.map(function(cur) {
                        return {
                            id: cur.id,
                            label: cur.fullName
                        }
                    });
                });
            }

            this.selectMention = function (item) {
                this.selectedMentions.push({id: item.id, fullName: item.label});

                return `@${item.label.replace(' ', '_')}`;
            }

            this.getValidatedMentions = function (messageBody) {
                var parsedNamesFromTextBody = parseMentions(messageBody);

                parsedNamesFromTextBody = removeDuplicates(parsedNamesFromTextBody);
                this.selectedMentions = removeDuplicates(this.selectedMentions, 'fullName');

                if (!parsedNamesFromTextBody || !this.selectedMentions) {
                    return undefined;
                }

                var mentionedUserIds = [];

                this.selectedMentions.forEach(function(cur) {
                    if(parsedNamesFromTextBody.includes(cur.fullName)) {
                        mentionedUserIds.push(cur.id);
                    }
                });

                return mentionedUserIds;
            }
        }

        function getUsersForAutocomplete(query) {
            return wallCommentRepository.getUsersForAutoComplete(query);
        }

        function parseMentions (text) {
            var pattern = /\B@[\u00BF-\u1FFF\u2C00-\uD7FF\w]+/gi;
            var matches = text.match(pattern);

            if (!matches) {
                return;
            }

            matches = removeDuplicates(matches);

            return matches.map(cur => {
                return cur.replace('@', '')
                         .replace('_', ' ');
            });
        }

        function removeDuplicates(myArr, prop) {
            if (!myArr) {
                return undefined;
            }

            return myArr.filter((obj, pos, arr) => {
                return prop ?
                    arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos :
                    arr.map(mapObj => mapObj).indexOf(obj) === pos;
            });
        }
    }
})();
