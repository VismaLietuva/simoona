(function () {
    'use strict';

    angular.module('simoonaApp.Settings')
           .controller('settingsProvidersController', settingsProvidersController);

    settingsProvidersController.$inject = [
        '$window',
        'authService',
        'notifySrv',
        'settingsRepository',
        'endPoint',
        '$location',
        'localeSrv',
        'errorHandler'
    ];

    function settingsProvidersController($window, authService, notifySrv, settingsRepository,
        endPoint, $location, localeSrv, errorHandler) {

        var vm = this;

        vm.isLoading = true;
        vm.organizationName = authService.getOrganizationNameFromUrl() || '';
        vm.endPoint = endPoint;
        vm.externalProviders = null;
        vm.internalProviders = null;

        vm.isGoogleLinked = false;
        vm.isFacebookLinked = false;
        vm.isMicrosoftLinked = false;
        vm.isInternalLinked = false;
        vm.googleEmail = null;
        vm.showGoogleEmail = false;
        vm.facebookEmail = null;
        vm.showFacebookEmail = false;
        vm.microsoftEmail = null;
        vm.showMicrosoftEmail = false;
        vm.internalEmail = null;
        vm.showInternalEmail = false;

        vm.providerSignIn = providerSignIn;
        vm.providerUnlink = providerUnlink;

        vm.isUnlinkEnabled = isUnlinkEnabled;

        vm.isGoogleProvider = false;
        vm.isFacebookProvider = false;
        vm.isMicrosoftProvider = false;
        vm.isInternalProvider = false;
        vm.activeProvider = null;

        vm.noExternalProviders = localeSrv.formatTranslation("applicationUser.noExternalProviders");

        init();

        ///////

        function init() {
            if (vm.internalProviders == null && vm.organizationName){
                authService.getInternalLogins().then(function(result) {
                    vm.internalProviders = result;
                    if (authService.getExternalProvider(vm.internalProviders, 'Internal')){
                        vm.isInternalProvider = true;
                    }

                }, function(error) {
                    errorHandler.handleErrorMessage(error);
                });
            }

            if (vm.externalProviders == null && vm.organizationName){
                authService.getLinkableLogins(getUrl()).then(function(result) {
                    vm.externalProviders = result;
                    if (authService.getExternalProvider(vm.externalProviders, 'Google')){
                        vm.isGoogleProvider= true;
                    }
                    if (authService.getExternalProvider(vm.externalProviders, 'Facebook')){
                        vm.isFacebookProvider = true;
                    }
                    if (authService.getExternalProvider(vm.externalProviders, 'Microsoft')){
                        vm.isMicrosoftProvider = true;
                    }
                }, function(error) {
                    errorHandler.handleErrorMessage(error);
                });
            }

            settingsRepository.getUserLogins().then(function (response) {
                setLoginsData(response);
                checkStatus();
                vm.isLoading = false;
            }, function(error){
                errorHandler.handleErrorMessage(error);
            });
        }

        function providerSignIn(providerName) {
            vm.activeProvider = providerName;
            var provider = authService.getExternalProvider(vm.externalProviders, providerName);
            if (vm.externalProviders) {
                $window.location.href = endPoint + provider.url;
            } else {
                notifySrv.error('settings.providerLoginFailed');
            }
        }

        function providerUnlink(providerName) {
            settingsRepository.deleteUserLogin(providerName).then(function () {
                var message = localeSrv.formatTranslation('settings.providerUnlinked', {one: providerName});
                notifySrv.success(message);
                getUserLogins();
            }, function (response) {
                notifySrv.error(response.data.message);
            });
        }

        function checkStatus() {
            var providerName = $location.search().authType;
            var status = $location.url().split("#")[1];
            if (status === "error=Access_denied" || status === "emailError=Access_denied"){
                notifySrv.error(localeSrv.formatTranslation('settings.providerLoginFailed'));
                $location.url($location.path());
            }
            else if (providerName){
                notifySrv.success(localeSrv.formatTranslation('settings.providerLinked', {one: providerName}));
                $location.url($location.path());
            }
        }

        function getUserLogins(){
            settingsRepository.getUserLogins().then(function (response) {
                setLoginsData(response);
            }, onError);
        }

        function setLoginsData(data){
            vm.logins = data;

            var res = checkIfLoginExists(vm.logins, 'Google');
            if (res != null){
                vm.isGoogleLinked = res.linked;
                if (res.email != null){
                    vm.showGoogleEmail = true;
                    vm.googleEmail = res.email;
                }
            } else {
                vm.isGoogleLinked = false;
                vm.showGoogleEmail = false;
                vm.googleEmail = null;
            }

            res = checkIfLoginExists(vm.logins, 'Facebook');
            if (res != null){
                vm.isFacebookLinked = res.linked;
                if (res.email != null){
                    vm.showFacebookEmail = true;
                    vm.facebookEmail = res.email;
                }
            } else {
                vm.isFacebookLinked = false;
                vm.showFacebookEmail = false;
                vm.facebookEmail = null;
            }

            res = checkIfLoginExists(vm.logins, 'Microsoft');
            if (res != null){
                vm.isMicrosoftLinked = res.linked;
                if (res.email != null){
                    vm.showMicrosoftEmail = true;
                    vm.showMicrosoftEmail = res.email;
                }
            } else {
                vm.isMicrosoftLinked = false;
                vm.showMicrosoftEmail = false;
                vm.microsoftEmail = null;
            }

            res = checkIfLoginExists(vm.logins, 'Internal');
            if (res != null){
                vm.isInternalLinked = res.linked;
                if (res.email != null){
                    vm.showInternalEmail = true;
                    vm.internalEmail = res.email;
                }
            } else {
                vm.isInternalLinked = false;
                vm.showInternalEmail = false;
                vm.internalEmail = null;
            }

            vm.unlinkEnabled = isUnlinkEnabled();
            if (vm.unlinkEnabled == false){
                if (vm.isInternalLinked){
                    vm.activeProvider = "Internal";
                } else if(vm.isGoogleLinked){
                    vm.activeProvider = 'Google';
                } else if(vm.isFacebookLinked) {
                    vm.activeProvider = 'Facebook';
                } else if(vm.isMicrosoftLinked) {
                    vm.activeProvider = 'Microsoft';
                }
            }
        }

        function isUnlinkEnabled() {
            var links = 0;
            if (vm.isGoogleLinked) {
                links++;
            }
            if (vm.isFacebookLinked) {
                links++;
            }
            if (vm.isMicrosoftLinked) {
                links++;
            }
            if (vm.isInternalLinked) {
                links++;
            }
            var providers = 0;
            if (vm.isGoogleLinked) {
                providers++;
            }
            if (vm.isFacebookLinked) {
                providers++;
            }
            if(vm.isMicrosoftLinked) {
                providers++;
            }
            if (vm.isInternalLinked) {
                providers++;
            }
            return (links > 1) && (providers > 1);
        }

        function checkIfLoginExists(logins, providerName) {
            for (var i = 0; i< logins.length; i++) {
                if (logins[i].loginProvider === providerName){
                    return {
                        linked: true,
                        email: logins[i].email
                    }
                }
            }
            return null;
        }

        function getUrl() {
            var returnUrl = $location.protocol() + '://' + $location.host();
            if ($location.port() !== '80') {
                returnUrl += ':' + $location.port();
            }

            return returnUrl += '/' + vm.organizationName + '/Settings/Providers';
        }

        function onError(response) {
            notifySrv.error(response.data);
        }
    }
}());
