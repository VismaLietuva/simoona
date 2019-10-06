(function() {
    'use strict';

    angular.module('simoonaApp.Lotteries')
        .controller('lotteryManageController', lotteryManageController);

    lotteryManageController.$inject = ['$scope', '$state', 'lotteryFactory', '$rootScope', 'notifySrv', '$q', 'localeSrv', 'errorHandler'
    ];

    function lotteryManageController($scope, $state, lotteryFactory, $rootScope, notifySrv, $q, localeSrv, errorHandler) {
        
        var vm = this;
        vm.openDatePicker = openDatePicker;
        vm.startLottery = startLottery;
        vm.datePicker = {
            isOpen: false
        };

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Admin.Lotteries.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Lotteries.Edit')
        };

        if (vm.states.isEdit) {
            setTitleScope(true, false, 'role.editRole');
        } else if (vm.states.isCreate) {
            setTitleScope(false, true, 'role.createRole');
        }

        function setTitleScope(titleEdit, titleCreate, pageTitle) {
            $scope.titleEdit = titleEdit;
            $scope.titleCreate = titleCreate;
            $rootScope.pageTitle = pageTitle;
        }

        function openDatePicker($event, datePicker) {
            $event.preventDefault();
            $event.stopPropagation();

            vm.datePicker.isOpen = true;
            $timeout(function() {
                $event.target.focus();
            }, 100);
        }

        function startLottery() {
            vm.lottery.status = 2;
            lotteryFactory.create(vm.lottery)
        }

    };
})();
