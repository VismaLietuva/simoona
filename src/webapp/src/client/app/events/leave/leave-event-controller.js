(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .controller('eventLeaveController', eventLeaveController);

        eventLeaveController.$inject = [
            '$uibModalInstance',
            'event',
            'leaveEvent',
            'notInterested',
            'attendStatus'
    ];

    function eventLeaveController($uibModalInstance, event, leaveEvent, notInterested, attendStatus) {
        /* jshint validthis: true */
        var vm = this;

        vm.event = event;
        vm.leave = leave;
        vm.closeModal = closeModal;

        init();
        
        //////

        function init() {
        }

        function leave() {
            if (!vm.comment)
            {
                vm.comment = "";
            }
            vm.event.participatingStatus == attendStatus.Attending ? leaveEvent(event.id, vm.comment) : notInterested(event.id, vm.comment);
            $uibModalInstance.close();
        }

        function closeModal() {
            $uibModalInstance.close();
        }
    }
})();
