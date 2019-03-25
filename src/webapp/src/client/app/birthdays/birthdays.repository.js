(function () {
    'use strict';

    var simoonaApp = angular.module('simoonaApp.Birthdays');

    simoonaApp.factory('birthdaysRepository', birthdaysRepository);

    birthdaysRepository.$inject = ['$resource', '$window', '$http', 'notifySrv', 'endPoint'];

    function birthdaysRepository($resource, $window, $http, notifySrv, endPoint) {
        var birthdaysUrl = endPoint + '/Birthdays/';

        return {
            getUsers: function () {
                return $resource(birthdaysUrl + 'GetWeeklyBirthdays', '', { 'query': { method: 'GET', isArray: true} }).query().$promise;
            }
        }
    }
})();
