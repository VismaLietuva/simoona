(function() {
    'use strict';

    angular
        .module('simoonaApp.Auth')
        .factory('authService', authService);

    authService.$inject = [
        '$resource',
        '$q',
        '$location',
        '$state',
        '$window',
        'appConfig',
        'localStorageService',
        'lodash',
        'localeSrv',
        'endPoint',
        '$http'
    ];

    function authService($resource, $q, $location, $state, $window,
        appConfig, localStorageService, lodash, localeSrv, endPoint, $http) {

        var accountUrl = endPoint + '/Account/';
        var applicationUrl = endPoint + '/ApplicationUser/';
        var tokenUrl = endPoint + '/token/';
        var organizationName = '';

        var authorizationData = {
            isAuthenticated: false,
            fullName: '',
            userName: '',
            roles: [],
            permissions: [],
            userId: '',
            organizationName: '',
            organizationId: '',
            impersonated: false,
            hasCompletedWalkThrough: false
        };

        init();

        var service = {
            getUserInfo: getUserInfo,
            registerExternal: registerExternal,
            register: register,
            requestToken: requestToken,
            signIn: signIn,
            requestPasswordReset: requestPasswordReset,
            resetPassword: resetPassword,
            verifyEmail: verifyEmail,
            getExternalLogins: getExternalLogins,
            getInternalLogins: getInternalLogins,
            getLinkableLogins: getLinkableLogins,
            getExternalProvider: getExternalProvider,
            getHashArrayFromUrl: getHashArrayFromUrl,
            getOrganizationNameFromUrl: getOrganizationNameFromUrl,
            getTokenFromUrl: getTokenFromUrl,
            getUserNameFromUrl: getUserNameFromUrl,
            isAuthenticated: isAuthenticated,
            isAuthenticatedNotNewUser: isAuthenticatedNotNewUser,
            setAuthenticationData: setAuthenticationData,
            logOut: logOut,
            isInRole: isInRole,
            hasPermissions: hasPermissions,
            hasOneOfPermissions: hasOneOfPermissions,
            identity: authorizationData,
            impersonate: impersonate,
            revertImpersonate: revertImpersonate,
            refreshUserAuthData: refreshUserAuthData,
            redirectToDefaultOrganization: redirectToDefaultOrganization,
            redirectToLogin: redirectToLogin,
            redirectToRegister: redirectToRegister,
            redirectToForgotPassword: redirectToForgotPassword,
            redirectToAccessDenied: redirectToAccessDenied,
            redirectToPageNotFound: redirectToPageNotFound,
            redirectToHome: redirectToHome,
            redirectToEvents: redirectToEvents,
            setOrganizationName: setOrganizationName,
            getOrganizationName: getOrganizationName,
            completeWalkThrough: completeWalkThrough,
            walkThroughCompletionStatus: walkThroughCompletionStatus
        };
        return service;

        //////

        function init() {
            if (!authorizationData.userName.length) {
                var authData = localStorageService.get('authorizationData');
                if (authData) {
                    angular.extend(authorizationData, authData);
                }
            }
        }

        function getHashArrayFromUrl() {
            var hash = $location.hash();

            if (hash) {
                var hashArray = [];
                var hashParams = hash.split('&');

                for (var i = 0; hashParams.length > i; i++) {
                    var pair = hashParams[i].split('=');
                    hashArray[pair[0]] = pair[1];
                }

                return hashArray;
            }

            return [];
        }

        function getOrganizationNameFromUrl() {
            var pathArray = $location.path().split('/');
            if (pathArray[1] && pathArray[1] !== 'Login') {
                return pathArray[1];
            } else {
                return false;
            }
        }

        function getTokenFromUrl() {
            var urlParams = new URLSearchParams($location.search());
            var token = urlParams.get('Token');

            return token;
        }

        function getUserNameFromUrl() {
            var urlParams = new URLSearchParams($location.search());
            var userName = urlParams.get('UserName');

            return userName;
        }

        function setOrganizationName(name) {
            organizationName = name;
        }

        function getOrganizationName() {
            return organizationName;
        }

        function refreshUserAuthData() {
            getUserInfo(authorizationData.token).then(function(response) {
                localeSrv.setLocale(response.cultureCode);

                if (response.hasRegistered) {
                    var authPermissions = angular.copy(authorizationData.permissions);
                    var responsePermissions = angular.copy(response.permissions);

                    if (!lodash.isEqual(authPermissions.sort(), responsePermissions.sort())) {
                        setAuthenticationData(response, authorizationData.token);
                        $window.location.reload();
                    }
                }
            });
        }

        function setAuthenticationData(response, token) {
            localeSrv.setLocale(response.cultureCode);
            service.identity = authorizationData = {
                token: token,
                isAuthenticated: true,
                fullName: response.fullName,
                userName: response.userName,
                roles: response.roles,
                permissions: response.permissions,
                userId: response.userId,
                organizationName: response.organizationName,
                organizationId: response.organizationId,
                impersonated: response.impersonated,
                hasCompletedWalkThrough: response.isTutorialComplete,
                pictureId: response.pictureId
            };

            localStorageService.set('authorizationData', authorizationData);
        }

        function registerExternal(token) {
            return $resource(accountUrl + 'RegisterExternal', {}, {
                save: {
                    withCredentials: true,
                    method: 'POST',
                    headers: {
                        'Authorization': 'Bearer ' + token,
                        'Access-Control-Allow-Credentials': true,
                    }
                }
            }).save().$promise;
        }

        function register(params) {
            return $resource(accountUrl + 'Register', {}, {
                post: {
                    withCredentials: true,
                    method: 'POST'
                }
            }).post(params).$promise;
        }

        function requestToken(params) {
            const data = "grant_type=password" +
                "&username=" + params.username +
                "&password=" + params.password +
                "&client_id=" + appConfig.clientId;

            return $resource(tokenUrl, {}, {

                post: {
                    withCredentials: true,
                    method: 'POST'
                }
            }).post(data).$promise;
        }

        function signIn(params) {
            params.clientId = appConfig.clientId;

            return $resource(accountUrl + 'SignIn', {}, {
                post: {
                    withCredentials: true,
                    method: 'POST'
                }
            }).post(params).$promise;
        }

        function requestPasswordReset(params) {
            return $resource(accountUrl + 'RequestPasswordReset', {}, {
                post: {
                    withCredentials: true,
                    method: 'POST'
                }
            }).post(params).$promise;
        }

        function resetPassword(params) {
            return $resource(accountUrl + 'ResetPassword', {}, {
                post: {
                    withCredentials: true,
                    method: 'POST'
                }
            }).post(params).$promise;
        }

        function verifyEmail(params) {
            return $resource(accountUrl + 'VerifyEmail', {}, {
                post: {
                    withCredentials: true,
                    method: 'POST'
                }
            }).post(params).$promise;
        }

        function getExternalLogins(returnUrl) {
            return $resource(accountUrl + 'ExternalLogins?returnUrl=' + returnUrl).query().$promise;
        }

        function getLinkableLogins(returnUrl) {
            return $resource(accountUrl + 'ExternalLogins?returnUrl=' + returnUrl + '&isLinkable=true').query().$promise;
        }

        function getInternalLogins() {
            return $resource(accountUrl + 'InternalLogins').query().$promise;
        }

        function logOut() {
            var authData = localStorageService.get('authorizationData');
            if (!!authData) {
                $resource(accountUrl + 'Logout', {}, {
                    save: {
                        withCredentials: true,
                        method: 'POST',
                        headers: {
                            'Authorization': 'Bearer ' + authData.token
                        }
                    }
                }).save();
            }

            localeSrv.setLocale("en-US");
            localStorageService.remove('authorizationData');

            var organizationName = authorizationData.organizationName;

            service.identity = authorizationData = {
                token: null,
                isAuthenticated: false,
                fullName: '',
                userName: '',
                roles: [],
                permissions: [],
                userId: '',
                organizationName: '',
                organizationId: '',
                impersonated: false,
                hasCompletedWalkThrough: false
            };

            $state.go('Root.WithOrg.Home',{
                organizationName : organizationName
            });
        }

        function isInRole(role) {
            return authorizationData.roles.contains(role);
        }

        function hasPermissions(permissions) {
            return lodash.every(permissions, function (permission) {
                return authorizationData.permissions.contains(permission);
            });
        }

        function hasOneOfPermissions(permissions) {
            return lodash.some(permissions, function (permission) {
                return authorizationData.permissions.contains(permission);
            });
        }

        function getExternalProvider(externalProviders, providerName) {
            return lodash.find(externalProviders, function (provider) {
                return provider.name === providerName;
            });
        }

        function getUserInfo(token) {
            return $resource(accountUrl + 'UserInfo', {}, {
                get: {
                    method: 'GET',
                    headers: {
                        Authorization: 'Bearer ' + token
                    }
                }
            }).get().$promise;
        }

        function impersonate(username) {
            return $resource(applicationUrl + 'Impersonate').get({
                username: username
            }).$promise;
        }

        function revertImpersonate(id) {
            return $resource(applicationUrl + 'RevertImpersonate').get().$promise;
        }

        function isAuthenticated() {
            return authorizationData.isAuthenticated;
        }

        function isAuthenticatedNotNewUser() {
            return authorizationData.isAuthenticated && !isInRole('NewUser');
        }

        function redirectToDefaultOrganization() {
            $state.go('Root.WithOrg.Login', {
                organizationName: appConfig.defaultOrganization
            }, {
                reload: false
            });
        }

        function redirectToLogin() {
            if (authorizationData.isAuthenticated) {
                logOut();
            }

            $state.go('Root.WithOrg.Login', {
                organizationName: organizationName,
                reload: true
            });
        }

        function redirectToRegister() {
            $state.go('Root.WithOrg.Register', {
                organizationName: organizationName,
                reload: true
            });
        }

        function redirectToForgotPassword() {
            $state.go('Root.WithOrg.Forgot', {
                organizationName: organizationName,
                reload: true
            });
        }

        function redirectToAccessDenied() {
            $state.go('Root.WithOrg.AccessDenied', {
                organizationName: authorizationData.organizationName
            }, {
                reload: true
            });
        }

        function redirectToPageNotFound(){
            $state.go('Root.WithOrg.PageNotFound', {
                organizationName: authorizationData.organizationName
            }, {
                reload: true
            });
        }

        function redirectToHome() {
            if (isInRole('NewUser')) {
                $state.go('Root.WithOrg.Client.Profiles.Edit', {
                    id:  authorizationData.userId,
                    organizationName: authorizationData.organizationName
                }, {
                    reload: true
                });
            } else {
                $state.go(appConfig.homeStateName, {
                    organizationName: authorizationData.organizationName
                }, {
                    reload: true
                });
            }
        }

        function redirectToEvents() {
            $state.go('Root.WithOrg.Client.Events.List.Type', {
                organizationName: authorizationData.organizationName
            }, {
                reload: true
            });
        }

        function completeWalkThrough() {
            return $resource(applicationUrl + 'CompleteTutorial', '', {
                put: {
                    method: 'PUT'
                }
            }).put().$promise;
        }

        function walkThroughCompletionStatus() {
            var deferred = $q.defer();

            $http({
                method: "GET",
                url: applicationUrl + 'TutorialStatus'
            }).then(function (response) {
                deferred.resolve(response);
            }, function (response) {
                deferred.reject("Rejected" + response.statusText);
            });
            return deferred.promise;
        }
    }
})();
