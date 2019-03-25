(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('wallController', wallController);

    wallController.$inject = [
        '$rootScope'
    ];

    function wallController($rootScope) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'wall.wallTitle';
    }
}());
