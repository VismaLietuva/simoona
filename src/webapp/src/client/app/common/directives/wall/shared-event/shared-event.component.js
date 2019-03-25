(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .component('aceWallSharedEvent', {
            bindings: {
                sharedEventId: '='
            },
            templateUrl: 'app/common/directives/wall/shared-event/shared-event.html',
            controller: wallSharedEventController,
            controllerAs: 'vm'
        });

    wallSharedEventController.$inject = [
        'wallSharedEventRepository',
        'errorHandler'
    ];

    function wallSharedEventController(wallSharedEventRepository, errorHandler) {
        /*jshint validthis: true */
        var vm = this;
        
        vm.eventDetails = {};
        vm.isLoading = true;
        vm.eventExists = true;

        /////////

        wallSharedEventRepository.getEventDetails(vm.sharedEventId).then(function (response) {
            vm.eventDetails = response;
            vm.isLoading = false;
        }, function(error) {
            var errorMsgCode = error.data.message;
            if (errorMsgCode === '206') {
                vm.eventExists = false;
                vm.isLoading = false;
            }
        });
    }

}());