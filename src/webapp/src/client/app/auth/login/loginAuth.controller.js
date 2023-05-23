(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .controller('loginAuthController', loginAuthController);

    loginAuthController.$inject = [
        '$rootScope',
        '$location',
        '$window',
        '$timeout',
        'authService',
        'notifySrv',
        'endPoint',
        'errorHandler',
        'localeSrv'
    ];

    function loginAuthController($rootScope, $location, $window, $timeout, authService, notifySrv, endPoint, errorHandler, localeSrv) {
        /* jshint validthis: true */
        var vm = this;
        var hashArray = authService.getHashArrayFromUrl();
        var access_token = hashArray['access_token'];
        var redirect = $location.search().redirect;

        vm.organizationName = authService.getOrganizationNameFromUrl() || '';
        vm.endPoint = endPoint;
        vm.externalProviders = null;
        vm.internalProviders = null;
        vm.isLoggingIn = !!access_token || authService.identity.isAuthenticated;
        $rootScope.pageTitle = 'account.login';
        vm.isInternal = false;
        vm.providerName;
        vm.availableProviders = [];

        vm.email = '';
        vm.password = '';

        vm.loginProvider = loginProvider;
        vm.providerSignIn = providerSignIn;
        vm.setProviderName = setProviderName;
        vm.providerRegister = providerRegister;
        vm.register = register;
        vm.redirectToForgotPassword = redirectToForgotPassword;
        vm.signIn = signIn;

        vm.newEmployee = null;
        vm.registerWith = null;
        vm.or = null;
        vm.with = null;
        vm.here = null;
        vm.forgotPassword = null;
        vm.rememberMe = null;

        init();

        ///////

        function init() {
            authService.setOrganizationName(vm.organizationName);
            if (vm.externalProviders == null && vm.organizationName){
                authService.getExternalLogins(getUrl()).then(function(result) {
                    vm.externalProviders = result;
                    if (authService.getExternalProvider(vm.externalProviders, 'Google')){
                        vm.availableProviders.push({name: 'Google', registerName: 'GoogleRegistration', singInTranslation: 'applicationUser.signInWithGoogle'});
                    }
                    if (authService.getExternalProvider(vm.externalProviders, 'Facebook')){
                        vm.availableProviders.push({name: 'Facebook', registerName: 'FacebookRegistration', singInTranslation: 'applicationUser.signInWithFacebook'});
                    }
                    if (authService.getExternalProvider(vm.externalProviders, 'Microsoft')){
                        vm.availableProviders.push({name: 'Microsoft', registerName: 'MicrosoftRegistration', singInTranslation: 'applicationUser.signInWithMicrosoft'});
                    }
                }, function(error) {
                    vm.isLoading = false;
                    errorHandler.handleErrorMessage(error);
                });
            }

            if (vm.internalProviders == null && vm.organizationName){
                authService.getInternalLogins().then(function(result) {
                    vm.internalProviders = result;
                    if (authService.getExternalProvider(vm.internalProviders, 'Internal')){
                        vm.isInternal = true;
                    }
                    $timeout(function () {
                        checkStatus();
                        loadStrings();
                    });
                }, function(error) {
                    vm.isLoading = false;
                    errorHandler.handleErrorMessage(error);
                });
            }

            if (access_token) {
                authService.getUserInfo(access_token).then(function(response) {
                    if (response.hasRegistered) {
                        authService.setAuthenticationData(response, access_token);
                        if (redirect){
                            $window.location.href = redirect;
                        }
                        if(sessionStorage.getItem("redirectAfterFailedLogin"))
                        {
                            $window.location.href = sessionStorage.getItem("redirectAfterFailedLogin");
                            sessionStorage.removeItem("redirectAfterFailedLogin");
                        }
                        authService.redirectToHome();
                    } else {
                        registerExternal(response);
                    }
                });
            } else if (authService.identity.isAuthenticated){
                if (redirect){
                    $window.location.href = redirect;
                }
                authService.redirectToHome();
            }
        }

        function setProviderName(providerName){
            vm.providerName = providerName;
        }

        function signIn() {
            if (!vm.email.length || !vm.password.length) {
                notifySrv.error('errorCodeMessages.messageAllFieldsAreRequired');
                return;
            }

            const signInInfo = {
                username: vm.email,
                password: vm.password
            };

            authService.requestToken(signInInfo).then(function(response) {
                authService.getUserInfo(response.access_token).then(function(info) {
                    authService.setAuthenticationData(info, response.access_token);
                    authService.signIn(signInInfo);

                    if (redirect) {
                        $window.location.href = redirect;
                    }
                    if(sessionStorage.getItem("redirectAfterFailedLogin"))
                    {
                        $window.location.href = sessionStorage.getItem("redirectAfterFailedLogin");
                        sessionStorage.removeItem("redirectAfterFailedLogin");
                    }
                    authService.redirectToHome();
                });
            }).catch(function(error) {
                if (error.data.error === 'not_verified') {
                    notifySrv.error("applicationUser.emailNotVerified");
                } else {
                    notifySrv.error("applicationUser.incorrectPasswordOrUserName");
                }
            });
        }

        function providerSignIn() {
            var provider = authService.getExternalProvider(vm.externalProviders, vm.providerName);
            vm.loginProvider(provider.url);
        }

        function registerExternal(userInfo) {
            authService.getExternalLogins(getUrl()).then(function(externalProviders) {
                authService.registerExternal(access_token).then(function() {
                    var provider = authService.getExternalProvider(externalProviders, userInfo.loginProvider);
                    $window.location.href = endPoint + provider.url;
                });
            }, errorHandler.handleErrorMessage);
        }

        function loginProvider(providerUrl) {
            if (vm.externalProviders) {
                $window.location.href = endPoint + providerUrl;
            } else {
                notifySrv.error('errorCodeMessages.messageOrganizationNotFound');
            }
        }

        function providerRegister(name) {
            var provider = authService.getExternalProvider(vm.externalProviders, name);
            vm.loginProvider(provider.url);
        }

        function register() {
            authService.redirectToRegister();
        }

        function redirectToForgotPassword() {
            authService.redirectToForgotPassword();
        }

        function checkStatus(){
            if (hashArray['notFound']) {
                notifySrv.error('account.notFound');
            }
            if (hashArray['error']) {
                notifySrv.error('account.loginFailure');
            }
            if (hashArray['emailError']) {
                notifySrv.error('account.emailFailure');
            }
            if (hashArray['providerExists']) {
                notifySrv.error('account.providerExists');
            }
        }

        function loadStrings(){
            vm.newEmployee = localeSrv.formatTranslation("applicationUser.newEmployeeText", {one: vm.organizationName});
            vm.registerWith = localeSrv.formatTranslation("applicationUser.registerWith");
            vm.or = localeSrv.formatTranslation("applicationUser.or");
            vm.with = localeSrv.formatTranslation("applicationUser.with");
            vm.here = localeSrv.formatTranslation("applicationUser.here");
            vm.forgotPassword = localeSrv.formatTranslation("applicationUser.forgotPassword");
            vm.rememberMe = localeSrv.formatTranslation("applicationUser.rememberMe");
        }

        function getUrl() {
            var returnUrl = $location.protocol() + '://' + $location.host();
            if ($location.port() !== '80') {
                returnUrl += ':' + $location.port();
            }

            returnUrl += '/' + vm.organizationName + '/Login';
            if (redirect) returnUrl += '?redirect=' + redirect;
            return returnUrl;
        }
    }
})();
