(function () {
    'use strict';

    angular.module('simoonaApp.ChatBot', []).config(config);

    config.$inject = ['$windowProvider'];

    function config($windowProvider) {
        if (!$windowProvider.$get().isChatBotEnabled) {
            return;
        }
    }
})();
