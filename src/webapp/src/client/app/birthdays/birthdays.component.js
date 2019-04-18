(function() {
    'use strict';

    angular
        .module('simoonaApp.Birthdays')
        .component('aceBirthdays', {
            templateUrl: 'app/birthdays/birthdays.html',
            controller: birthdaysController,
            controllerAs: 'vm',
            bindings: {
                users: '<'
            }
        });

    birthdaysController.$inject = [
        '$locale',
        'authService',
        'smallAvatarThumbSettings'
    ];

    function birthdaysController($locale, authService, smallAvatarThumbSettings) {
        /*jshint validthis: true */
        var vm = this;

        vm.users = [];
        vm.userThumbSettings = smallAvatarThumbSettings;

        init();

        ///////

        function init() {
            if (authService.hasPermissions(['BIRTHDAYS_BASIC'])) {
                getMonths();
            }
        }

        function getMonths() {
            var date = new Date();
            var datetime = $locale.DATETIME_FORMATS;

            vm.months = datetime.MONTH;
            vm.monthToExport = vm.months[date.getMonth()];
        }
    }
})();
