(function() {
    'use strict';

    angular.module('simoonaApp.Lotteries')
        .constant('lotteryImageSettings', {
            height: 720,
            width: 1224,
        })
        .controller('lotteryManageController', lotteryManageController);

    lotteryManageController.$inject = ['$state', 'lotteryRepository', '$rootScope',
    'notifySrv', '$q', 'localeSrv', 'errorHandler', 'lotteryStatuses', 'lottery', 'pictureRepository', 'dataHandler', 'lotteryImageSettings', '$timeout'
    ];

    function lotteryManageController($state, lotteryRepository, $rootScope, notifySrv, $q, localeSrv, errorHandler,
        lotteryStatuses, lottery, pictureRepository, dataHandler, lotteryImageSettings, $timeout) {

        var vm = this;
        vm.openDatePicker = openDatePicker;
        vm.startLottery = startLottery;
        vm.createLottery = createLottery;
        vm.updateLottery = updateLottery;
        vm.abortLottery = abortLottery;
        vm.finishLottery = finishLottery;
        vm.removeImage = removeImage;
        vm.lotteryCroppedImages = [];

        vm.lotteryImageSize = {
            w: lotteryImageSettings.width,
            h: lotteryImageSettings.height
        };

        vm.datePicker = {
            isOpen: false
        };

        vm.states = {
            isCreate: $state.includes('Root.WithOrg.Admin.Lotteries.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Lotteries.Edit')
        };

        vm.isPrimaryEditDisabled = isPrimaryEditDisabled();
        vm.lotteryStatuses = lotteryStatuses;

        setTitleScope(vm.states);

        function setTitleScope(states) {
            if (states.isEdit) {
                vm.lottery = lottery;
                vm.lottery.endDate = moment.utc(vm.lottery.endDate).local().startOf('minute').toDate();
                vm.isStarted = vm.lottery.status === lotteryStatuses.started;
                vm.isDrafted = vm.lottery.status === lotteryStatuses.drafted;
                vm.isEndedByTime = vm.lottery.status === lotteryStatuses.endedByTime;
                vm.isEnded = (vm.lottery.status !== lotteryStatuses.drafted) && (vm.lottery.status !== lotteryStatuses.started);
                $rootScope.pageTitle = 'lotteries.editLottery';
            } else if (states.isCreate) {
                $rootScope.pageTitle = 'lotteries.createLottery';
            }
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
            saveimages()
                .then(results => {
                    vm.lottery.images = results;
                    vm.lottery.status = lotteryStatuses.started;
                    lotteryRepository.create(vm.lottery)
                    .then(function() {
                        notifySrv.success(localeSrv.formatTranslation('lotteries.hasStarted', { one: 'lotteries.entityNameSingular', two: vm.lottery.title }));
                        $state.go('^.List');
                    })
                })

        }

        function createLottery() {
            saveimages()
                .then(results => {
                    vm.lottery.status = lotteryStatuses.drafted;
                    vm.lottery.images = results;
                    lotteryRepository.create(vm.lottery)
                        .then(updateSucess('lotteries.hasBeenSaved'))
                });
        }

        function saveimages() {
            var uploads = vm.lotteryImages.map((image, index) => {
                var lotteryImageBlob = dataHandler.dataURItoBlob(vm.lotteryCroppedImages[index], image.type);
                lotteryImageBlob.lastModifiedDate = new Date();
                lotteryImageBlob.name = image.name;
                return pictureRepository.upload([lotteryImageBlob]);
            })

            return $q.all(uploads)
                .then(function(data) {
                    var images = []
                    data.forEach(result => {
                        images.push(result.data);
                    })
                    return data.map(result => result.data);
            });
        }

        function removeImage(lotteryImage) {
            var index = vm.lottery.images.indexOf(lotteryImage);
            if (index > -1) {
            vm.lottery.images.splice(index, 1);
            }
        }

        function updateLottery(start) {
            if (vm.isDrafted) {
                vm.lottery.status = start ? lotteryStatuses.started : lotteryStatuses.drafted;
                if (vm.lotteryCroppedImages) {
                    saveimages()
                        .then(newImages => {
                            vm.lottery.images = vm.lottery.images.concat(newImages);
                            lotteryRepository.updateDrafted(vm.lottery)
                                .then(updateSucess('lotteries.hasBeenSaved'))
                        })
                } else {
                    lotteryRepository.updateDrafted(vm.lottery)
                    .then(updateSucess('lotteries.hasBeenSaved'))
                }
            } else if (vm.isStarted) {
                lotteryRepository.updateStarted({ description: vm.lottery.description, id: vm.lottery.id })
                    .then(updateSucess('lotteries.hasBeenSaved'))
            }
        }

        function abortLottery(id) {
            lotteryRepository.abortLottery(id).then(function() {
                notifySrv.success('lotteries.successDelete');
                $state.go('Root.WithOrg.Admin.Lotteries.List');
            }, errorHandler.handleErrorMessage);
        }

        function finishLottery() {
            lotteryRepository.finishLottery(vm.lottery.id)
                .then(function() {
                    notifySrv.success(localeSrv.formatTranslation('lotteries.successFinish', { one: 'lotteries.entityNameSingular', two: vm.lottery.title }));
                    $state.go('Root.WithOrg.Admin.Lotteries.List');
                }, errorHandler.handleErrorMessage);
        }

        function updateSucess(translation) {
            notifySrv.success(localeSrv.formatTranslation(translation, { one: 'lotteries.entityNameSingular', two: vm.lottery.title }));
            $state.go('^.List');
        }

        function isPrimaryEditDisabled() {
            return vm.states.isCreate === false &&
                   (vm.states.isEdit && lottery.status === lotteryStatuses.drafted) === false;
        }
    };
})();
