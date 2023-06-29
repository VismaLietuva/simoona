(function () {
    'use strict';

    const simoonaApp = angular.module('simoonaApp.ChatBot');

    simoonaApp.factory('chatBotRepository', chatBotRepository);

    chatBotRepository.$inject = ['$http', 'chatBotEndpoint', 'chatBotAgentId'];

    function chatBotRepository($http, chatBotEndpoint, chatBotAgentId) {
        return {
            message: function (message, historyId) {
                return $http.post(chatBotEndpoint, {
                    agentId: chatBotAgentId,
                    chatHistoryId: historyId,
                    message: message
                })
            }
        }
    }
})();
