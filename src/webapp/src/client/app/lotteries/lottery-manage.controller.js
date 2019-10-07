(function() {
    'use strict';

    angular.module('simoonaApp.Lotteries')
        .constant('lotteryStatus', {
            Drafted: 1,
            Started: 2,
            Aborted: 3,
            Ended: 4
        })
        .controller('lotteryManageController', lotteryManageController);

    lotteryManageController.$inject = ['$scope', '$state', 'lotteryFactory', '$rootScope',
    'notifySrv', '$q', 'localeSrv', 'errorHandler', 'lotteryStatus', 'lottery'
    ];

    function lotteryManageController($scope, $state, lotteryFactory, $rootScope, notifySrv, $q, localeSrv, errorHandler, lotteryStatus, lottery) {
        
        var vm = this;
        vm.openDatePicker = openDatePicker;
        vm.startLottery = startLottery;
        vm.createLottery = createLottery;
        vm.updateLottery = updateLottery;
        vm.datePicker = {
            isOpen: false
        };

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Admin.Lotteries.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Lotteries.Edit')
        };

        if (vm.states.isEdit) {
            vm.lottery = lottery;
            vm.lottery.endDate = moment.utc(vm.lottery.endDate).local().startOf('minute').toDate();
            vm.isDrafted = vm.lottery.status === lotteryStatus.Drafted;
            vm.isStarted = vm.lottery.status === lotteryStatus.Started;
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
            vm.lottery.status = lotteryStatus.Started;
            lotteryFactory.create(vm.lottery)
                .then(function() {
                    notifySrv.success(localeSrv.formatTranslation('lotteries.hasStarted', { one: 'lotteries.entityNameSingular', two: vm.lottery.title }));
                    $state.go('^.List');
                })
        }

        function createLottery() {
            vm.lottery.status = lotteryStatus.Drafted;
            lotteryFactory.create(vm.lottery)
                .then(updateSucess())
        }

        function updateLottery() {
            if (vm.isDrafted) {
                lotteryFactory.updateDrafted(vm.lottery)
                    .then(updateSucess())
            } else if (vm.isStarted) {
                lotteryFactory.updateStarted({ description: vm.lottery.description, id: vm.lottery.id })
                    .then(updateSucess())
            }
        }

        function updateSucess() {
            notifySrv.success(localeSrv.formatTranslation('lotteries.hasBeenSaved', { one: 'lotteries.entityNameSingular', two: vm.lottery.title }));
            $state.go('^.List');
        }

    };
})();
