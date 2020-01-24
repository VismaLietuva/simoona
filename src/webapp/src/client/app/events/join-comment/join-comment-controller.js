(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .controller('joinCommentController', joinCommentController);

        joinCommentController.$inject = [
            '$uibModalInstance',
            'event',
            'updateEventStatus',
            'changeToAttendStatus',
            'attendStatus'
    ];

    function joinCommentController($uibModalInstance, event, updateEventStatus, changeToAttendStatus, attendStatus) {
        /* jshint validthis: true */
        var vm = this;

        vm.event = event;
        vm.updateStatus = updateStatus;
        vm.changeToAttendStatus = changeToAttendStatus;
        vm.attendStatus = attendStatus
        vm.closeModal = closeModal;

        init();
        
        //////

        function init() {
        }

        function updateStatus() {
            if (!vm.comment)
            {
                vm.comment = "";
            }

            updateEventStatus(vm.event.id, vm.changeToAttendStatus, vm.comment);
            $uibModalInstance.close();
        }

        function closeModal() {
            $uibModalInstance.close();
        }
    }
})();
