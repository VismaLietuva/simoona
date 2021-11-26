(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.ExternalLinks')
        .controller('externalLinksController', externalLinksController);

    externalLinksController.$inject = [
        '$rootScope',
        '$state',
        'lodash',
        'externalLinksRepository',
        'notifySrv',
        'errorHandler',
        'externalLinkTypes'
    ];

    function externalLinksController($rootScope, $state, lodash, externalLinksRepository, notifySrv, errorHandler, externalLinkTypes) {
        /*jshint validthis: true */
        var vm = this;

        $rootScope.pageTitle = 'customization.externalLinks';

        var tempLinkList = [];
        vm.externalLinks = [];
        vm.externalLinkTypes = [];
        vm.linksToDelete = [];

        vm.urlRegex = /https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&=]*)/;

        vm.addLink = addLink;
        vm.deleteLink = deleteLink;
        vm.isLinksUnique = isLinksUnique;
        vm.isLinkUnique = isLinkUnique;
        vm.saveLinks = saveLinks;
        vm.cancelLinksUpdate = cancelLinksUpdate;
        vm.isUpdated = isUpdated;
        vm.isLoadingLinks = true;


        init();

        function init() {
            addSelectionOptions();

            externalLinksRepository.getExternalLinks().then(function(response) {
                vm.externalLinks = response;
                tempLinkList = angular.copy(vm.externalLinks);
                vm.isLoadingLinks = false;
            });
        }

        function addLink() {
            vm.externalLinks.push({
                name: '',
                url: '',
                type: ''
            });
        }

        function deleteLink(index) {
            if (!!vm.externalLinks[index].id) {
                vm.linksToDelete.push(vm.externalLinks[index].id);
            }

            vm.externalLinks.splice(index, 1);
        }

        function saveLinks() {

            var externalLinksUpdateObject = {
                LinksToUpdate: [],
                LinksToCreate: [],
                LinksToDelete: []
            };

            if (!!vm.externalLinks.length) {

                vm.externalLinks.forEach(link => link.type = link.type.linkTypeValue);

                externalLinksUpdateObject.LinksToCreate = lodash.filter(vm.externalLinks, (element) => {
                    return !element.id;
                });

                externalLinksUpdateObject.LinksToUpdate = lodash.filter(vm.externalLinks, (element) => {
                    return !!element.id;
                });
            }

            externalLinksUpdateObject.LinksToDelete = vm.linksToDelete;

            externalLinksRepository.postExternalLinks(externalLinksUpdateObject).then(function(response) {
                notifySrv.success('common.successfullySaved');
                $state.go('Root.WithOrg.Admin.Customization.List');
            }, function(error) {
                errorHandler.handleErrorMessage(error);
            });
        }

        function cancelLinksUpdate() {
            $state.go('Root.WithOrg.Admin.Customization.List');
        }

        function addSelectionOptions() {
            for(const type in externalLinkTypes) {
                vm.externalLinkTypes.push({ linkType: type, linkTypeValue: externalLinkTypes[type] });
            }
        }

        function isLinksUnique() {
            if (vm.externalLinks.length > 0) {
                var tempArray = angular.copy(vm.externalLinks);
                var uniqueNames = lodash.uniqBy(tempArray, 'name');
                var uniqueLinks = lodash.uniqBy(uniqueNames, 'url');

                return lodash.isEqual(tempArray.sort(), uniqueLinks.sort());
            } else {
                return true;
            }
        }

        function isLinkUnique(linkObj, key) {
            if (!!linkObj[key]) {
                return isUniqueByKey(linkObj, key);
            } else {
                return true;
            }
        }

        function isUniqueByKey(linkObj, key) {
            var firstIndex, duplicateIndex;

            firstIndex = lodash.findIndex(vm.externalLinks, function(obj) {
                return obj[key] === linkObj[key];
            });

            duplicateIndex = lodash.findLastIndex(vm.externalLinks, function(obj) {
                return obj[key] === linkObj[key];
            });

            if (firstIndex !== duplicateIndex) {
                return false;
            } else {
                return true;
            }
        }

        function isUpdated() {
            return !lodash.isEqual(angular.copy(tempLinkList).sort(), angular.copy(vm.externalLinks).sort());
        }
    }
})();
