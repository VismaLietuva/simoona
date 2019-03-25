(function () {

    'use strict';

    angular
        .module('simoonaApp.Hashtags')
        .factory('hashtagRepository', hashtagRepository);

    hashtagRepository.$inject = ['$resource', '$q', 'endPoint'];

    function hashtagRepository($resource, $q, endPoint) {
        var hashtagUrl = endPoint + '/Hashtags/';
        var regex = XRegExp('(?:\\s|^)#([a-z0-9_\\p{L}]+)', 'g');

        var service = {
            addMention: addMention,
            removeMention: removeMention,
            findHashtags: findHashtags,
            findHashtagChanges: findHashtagChanges,
            removeMentionsOnDelete: removeMentionsOnDelete,
            removeMentionsOnDeletePost: removeMentionsOnDeletePost
        };
        return service;

        /////

        function addMention(model) {
            return $resource(hashtagUrl + 'AddHashtagMention', '', { Put: { method: 'PUT' } }).Put(model).$promise;
        }

        function removeMention(model) {
            return $resource(hashtagUrl + 'RemoveHashtagMention', {}, { delete: { method: 'DELETE' } }).delete(model).$promise;
        }

        function findHashtags(string) {
            var m;
            var array = [];
            var promisesArray = [];
            while (m = regex.exec(string)) {
                if (array.indexOf(m[0].trim()) <= -1) {
                    promisesArray.push(
                        addMention({ text: m[0].trim() })
                    );
                    array.push(m[0].trim());
                }
            }
            return $q.all(promisesArray);
        }

        function findHashtagChanges(originalString, editedString) {
            var m;
            var i;
            var originalArray = [];
            var editedArray = [];
            var promisesArray = [];
            while (m = regex.exec(originalString)) {
                if (originalArray.indexOf(m[0].trim()) <= -1) {
                    originalArray.push(m[0].trim());
                }
            }
            while (m = regex.exec(editedString)) {
                if (editedArray.indexOf(m[0].trim()) <= -1) {
                    editedArray.push(m[0].trim());
                }
            }
            for (i = 0; i < originalArray.length; i++) {
                if (editedArray.indexOf(originalArray[i]) <= -1) {
                    promisesArray.push(
                        removeMention({ hashtagText: originalArray[i] })
                    );
                }
            }
            for (i = 0; i < editedArray.length; i++) {
                if (originalArray.indexOf(editedArray[i]) <= -1) {
                    promisesArray.push(
                        addMention({ text: editedArray[i] })
                    );
                }
            }
            return $q.all(promisesArray);
        }

        function removeMentionsOnDelete(string) {
            var m;
            var array = [];
            var promisesArray = [];
            while (m = regex.exec(string)) {
                if (array.indexOf(m[0].trim()) <= -1) {
                    array.push(m[0].trim());
                }
            }
            for (var i = 0; i < array.length; i++) {
                promisesArray.push(
                    removeMention({ hashtagText: array[i] })
                );
            }

            return $q.all(promisesArray);
        }

        function removeMentionsOnDeletePost(post) {
            var associativeArray = {};
            var m;
            var i;
            var promisesArray = [];
            while (m = regex.exec(post.messageBody)) {
                if (!(m[0].trim() in associativeArray)) {
                    associativeArray[m[0].trim()] = 1;
                }
            }
            for (i = 0; i < post.comments.length; i++) {
                var array = [];
                while (m = regex.exec(post.comments[i].messageBody)) {
                    if (array.indexOf(m[0].trim()) <= -1) {
                        array.push(m[0].trim());
                        associativeArray[m[0].trim()] = associativeArray[m[0].trim()] + 1;
                    }
                }
            }
            for (i = 0; i < Object.keys(associativeArray).length; i++) {
                var key = Object.keys(associativeArray)[i];
                promisesArray.push(
                    removeMention({ hashtagText: key })
                );
            }
            return $q.all(promisesArray);
        }
    }
})();
