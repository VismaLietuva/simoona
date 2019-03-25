(function() {
    'use strict';

    angular
        .module('simoonaApp.Birthdays')
        .component('aceBirthdays', {
            templateUrl: 'app/birthdays/birthdays.html',
            controller: birthdaysController,
            controllerAs: 'vm'
        });

    birthdaysController.$inject = [
        '$locale',
        'authService',
        'birthdaysRepository',
        'Analytics',
        'smallAvatarThumbSettings'
    ];

    function birthdaysController($locale, authService, birthdaysRepository, Analytics, smallAvatarThumbSettings) {
        /*jshint validthis: true */
        var vm = this;

        vm.users = [];
        vm.userThumbSettings = smallAvatarThumbSettings;

        init();

        ///////

        function init() {
            if (authService.hasPermissions(['BIRTHDAYS_BASIC'])) {
                getUsers();
                getMonths();
            }
        }

        function getUsers() {
            birthdaysRepository.getUsers().then(function(response) {
                vm.users = response;
            });
        }

        function getMonths() {
            var date = new Date();
            var datetime = $locale.DATETIME_FORMATS;

            vm.months = datetime.MONTH;
            vm.monthToExport = vm.months[date.getMonth()];
        }
    }
})();
