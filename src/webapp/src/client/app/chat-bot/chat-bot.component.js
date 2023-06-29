(function () {
    'use strict';

    angular
        .module('simoonaApp.ChatBot')
        .component('aceChatBot', {
            templateUrl: 'app/chat-bot/chat-bot.html',
            controller: chatBotController,
            controllerAs: 'vm'
        });

    chatBotController.$inject = [
        'chatBotRepository',
        '$timeout',
        'localeSrv'
    ];

    function chatBotController(chatBotRepository, $timeout, localeSrv) {
        const vm = this;

        init();

        function init(){
            vm.messages = [];

            vm.historyId = crypto.randomUUID();

            vm.messages.push({
                text: localeSrv.formatTranslation('chatBot.initialMessage'),
                isBotMessage: true
            })
        }

        function sendMessageToApiAndUpdateChat(message) {
            chatBotRepository.message(message, vm.historyId).then(function (messageResponse) {
                vm.isLoading = false;
                vm.messages.pop();

                pushMessageToChat(messageResponse.data, true);

                $timeout(scrollChatWindowToBottom);
            });
        }

        function pushMessageToChat(message, isBotMessage) {
            vm.messages.push({
                text: message,
                isBotMessage: isBotMessage
            });
        }

        function scrollChatWindowToBottom() {
            const chatWindow = document.getElementById('chatMessages');
            chatWindow.scrollTop = chatWindow.scrollHeight;
        }

        vm.sendMessage = function () {
            if (vm.newMessage) {
                pushMessageToChat(vm.newMessage, false);

                const message = vm.newMessage;
                vm.newMessage = '';
                vm.isLoading = true;

                $timeout(scrollChatWindowToBottom);

                pushMessageToChat(localeSrv.formatTranslation('chatBot.thinking'), true);

                sendMessageToApiAndUpdateChat(message);
            }
        };

        vm.handleKeyPress = function (event) {
            const enterButtonCode = 13;
            if (event.keyCode === enterButtonCode && !vm.isLoading) {
                vm.sendMessage();
            }
        };
    }
})();
