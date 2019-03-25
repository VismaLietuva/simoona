(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceShareBtn', {
            bindings: {
                shareItemCategory: '@',
                shareItemId: '@'
            },
            templateUrl: 'app/common/directives/share-btn/share-btn.html',
            controller: shareButtonController,
            controllerAs: 'vm'
        });

    function shareButtonController() {
        /*jshint validthis: true */
        var vm = this;
    }
}());
