(function () {
    'use strict';

    const simoonaApp = angular.module('simoonaApp.ChatBot');

    simoonaApp.factory('chatBotRepository', chatBotRepository);

    chatBotRepository.$inject = ['$http', 'chatBotEndpoint', 'chatBotAgentId'];

    function chatBotRepository($http, chatBotEndpoint, chatBotAgentId) {
        return {
            message: function (message, conversationId) {
                return $http.post(chatBotEndpoint, {
                    agentId: chatBotAgentId,
                    conversationId: conversationId,
                    message: message
                })
            }
        }
    }
})();
