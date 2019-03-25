(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .directive('aceShareModal', aceShareModal)
        .constant('FORM_SETTINGS', {
        'DESCRIPTION_FIELD_LENGTH': '5000'
    });

    aceShareModal.$inject = [
        '$uibModal'
    ];

    function aceShareModal($uibModal) {
        return {
            restrict: 'A',
            scope: {
                aceShareModal: '@',
                shareModalConfirm: '@',
                shareModalReject: '@',
                shareModalTitle: '@',
                shareModalItemCategory: '@'
            },
            link: function (scope, elem) {
                elem.bind('click', function () {
                    $uibModal.open({
                        templateUrl: 'app/common/directives/share-btn/share-modal/share-modal.html',
                        controller: shareModalController,
                        controllerAs: 'vm',
                        resolve: {
                            shareItemId: function () {
                                return scope.aceShareModal;
                            },
                            shareModalReject: function () {
                                return scope.shareModalReject;
                            },
                            shareModalConfirm: function () {
                                return scope.shareModalConfirm;
                            },
                            shareModalTitle: function () {
                                return scope.shareModalTitle;
                            },
                            shareModalItemCategory: function () {
                                return scope.shareModalItemCategory;
                            }

                        }
                    });
                });
            }
        };
    }

    shareModalController.$inject = [
        '$uibModalInstance',
        'shareItemId',
        'shareModalItemCategory',
        'shareModalReject',
        'shareModalConfirm',
        'shareModalTitle',
        'wallService',
        'shareModalRepository',
        'notifySrv',
        'errorHandler',
        'FORM_SETTINGS'
    ];

    function shareModalController($uibModalInstance, shareItemId, shareModalItemCategory, shareModalReject, shareModalConfirm, shareModalTitle, wallService, shareModalRepository, notifySrv, errorHandler, FORM_SETTINGS) {
        /* jshint validthis: true */
        var vm = this;

        vm.shareModalTitle = shareModalTitle ? shareModalTitle : 'common.share';
        vm.shareModalReject = shareModalReject ? shareModalReject : 'common.cancel';
        vm.shareModalConfirm = shareModalConfirm ? shareModalConfirm : 'common.share';
        vm.shareModalSelectLabel = 'common.shareModalWallLabel';
        vm.shareModalDescriptionLabel = 'common.shareModalDescriptionLabel';
        
        vm.formSettings = FORM_SETTINGS;
        
        vm.walls = wallService.wallServiceData.wallList;
        vm.selectedWall;
        vm.description;
        vm.disableButton = false;

        vm.share = share;
        vm.closeModal = closeModal;
        
        var shareItemCategory = shareModalItemCategory.toLowerCase();
        
        var itemToShare = {
            wallId: null,
            messageBody: null
        };
        
        itemToShare[shareItemCategory + 'Id'] = shareItemId;

        ////////////

        function share() {
            vm.disableButton = true;
            updateItemToShareInfo();
            shareModalRepository.shareOnWall(itemToShare, shareItemCategory).then(function () {
                notifySrv.success('common.successfullyShared');
            }, errorHandler.handleErrorMessage).finally(function() {
                $uibModalInstance.close();
            });
        }

        function closeModal() {
            $uibModalInstance.close();
        }

        function updateItemToShareInfo() {
            itemToShare.wallId = vm.selectedWall.id;
            itemToShare.messageBody = vm.description;
        }

    }
})();