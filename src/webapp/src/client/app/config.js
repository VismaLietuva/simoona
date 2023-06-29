(function() {
    'use strict';
    var modulesList = [
        'ui.router',
        'ngResource',
        'ui.bootstrap',
        'ngCookies',
        'ngTagsInput',
        'ngAnimate',
        'ngSanitize',
        'toaster',
        'mentio',
        'timer',
        'bootstrapLightbox',
        'LocalStorageModule',
        'monospaced.elastic',
        'ApplicationInsightsModule',
        'SignalR',
        'pascalprecht.translate',
        'tmh.dynamicLocale',
        'angular-loading-bar',
        'angular-google-analytics',
        'simoonaApp.Constant',
        'simoonaApp.Layout',
        'simoonaApp.Common',
        'simoonaApp.Auth',
        'feature-flags',
        'angularMoment',
        //Shrooms modules
        'simoonaApp.Certificate',
        'simoonaApp.Picture',
        'simoonaApp.Wall',
        'simoonaApp.Support',
        'simoonaApp.Project',
        'simoonaApp.Role',
        'simoonaApp.RoomType',
        'simoonaApp.Users',
        'simoonaApp.Skill',
        'simoonaApp.Impersonate',
        'simoonaApp.Widget',
        'simoonaApp.Customization',
        'simoonaApp.Lotteries',
        'simoonaApp.Settings',
        'simoonaApp.ChatBot'
    ];

    window.modules = modulesList;
    window.isPremium = false; // Enable/disable premium modules
    window.lotteriesEnabled = false;
    window.usingAnimatedGifs = false; // Used to determine if back-end uses AnimatedGifs plugin
    window.usePostWatching = true; // Used to show/hide post watching feature

    angular.module('simoonaApp', window.modules)
        .run(execute)
        .config(configuration)
        .constant('LOCALES', {
            'locales': {
                'lt_LT': 'Lietuvių',
                'en_US': 'English'
            },
            'preferredLocale': 'en_US'
        })
        .constant('appConfig', {
            homeStateName: 'Root.WithOrg.Client.Wall.Item.Feed',
            adminRole: 'Admin',
            allowedStatesForNewUser: ['Root.WithOrg.Client.Profiles.Edit', 'Root.WithOrg.LogOff'],
            defaultOrganization: 'Organization',
            clientId: 'testingAngularClientId'
        })
        .constant('imageValidationSettings', {
            allowed: ['image/png', 'image/jpg', 'image/jpeg', 'image/gif'],
            sizeLimit: 5242880
        })
        .constant('smallAvatarThumbSettings', {
            width: 35,
            height: 35,
            mode: 'crop'
        })
        .constant('definedKudosTypes', {
            ordinary: 1,
            send: 2,
            minus: 3,
            other: 4,
            welcome: 5,
            refund: 6
        })
        .config(localesTranslations)
        .config(dynamicLocale);

    execute.$inject = ['$window', '$rootScope', '$state', '$stateParams',
        '$uibModalStack', 'authService', 'featureFlags', 'Analytics'
    ];

    function execute($window, $rootScope, $state, $stateParams,
        $uibModalStack, authService, featureFlags, Analytics) {

        featureFlags.set([{
            'key': 'premium',
            'active': window.isPremium,
            'name': 'Premium Features',
            'description': 'Premium Features'
        },
        {
            'key': 'animatedGifs',
            'active': window.usingAnimatedGifs,
            'name': 'AnimatedGifs',
            'description': 'Used to set if back-end uses AnimatedGifs plugin'
        },
        {
            'key': 'usePostWatching',
            'active': window.usePostWatching,
            'name': 'Posts watch/unwatch feature',
            'description': 'Used to mark post as being watched or not watched to receive notifications'
        }]);

        Analytics.pageView();

        $rootScope.$state = $state;
        $rootScope.$stateParams = $stateParams;

        $rootScope.goBack = function() {
            $window.history.back();
        };

        $rootScope.$on('$stateChangeSuccess', function(event, toState, toParams, fromState, fromParams) {
            document.body.scrollTop = document.documentElement.scrollTop = 0;
            //fix for modal staying on after navigation page while its open
            $uibModalStack.dismissAll();
        });

        if (authService.identity.isAuthenticated) {
            authService.refreshUserAuthData();
        }
    }

    configuration.$inject = [
        '$httpProvider',
        '$locationProvider',
        'cfpLoadingBarProvider',
        'ChartJsProvider',
        'AnalyticsProvider',
        'environment',
        'applicationInsightsServiceProvider'
    ];

    function configuration($httpProvider, $locationProvider, cfpLoadingBarProvider,
        ChartJsProvider, AnalyticsProvider, environment, applicationInsightsServiceProvider) {

        var options = {
            applicationName: '',
            appInsightsEnabled: false,
            appInsightsInstrumentationKey: 'appInsightsInstrumentationKeyValue'
        };

        if (!options.appInsightsEnabled)
        {
            options.autoPageViewTracking = false;
            options.autoStateChangeTracking = false;
            options.autoLogTracking = false;
            options.autoExceptionTracking = false;
        }

        applicationInsightsServiceProvider.configure(options.appInsightsInstrumentationKey, options);

        $locationProvider.html5Mode(true);

        AnalyticsProvider.useDisplayFeatures(true);
        AnalyticsProvider.trackPages(true);
        AnalyticsProvider.trackUrlParams(true);
        AnalyticsProvider.setPageEvent('$stateChangeSuccess');

        if (!!environment && environment === 'prod') {
            AnalyticsProvider.setAccount({
                tracker: 'trackerNumberProd',
                trackEvent: true,
                set: {
                    forceSSL: true
                }
            });
        } else {
            AnalyticsProvider.setDomainName('none');
            AnalyticsProvider.setAccount({
                tracker: 'trackerNumberNone',
                trackEvent: true,
                set: {
                    forceSSL: true
                }
            });
        }

        ChartJsProvider.setOptions({
            legend: { display: true, position: 'bottom' },
            elements: {
                line: { tension: 0, fill: false }
            }
        });

        ChartJsProvider.$get().Chart.defaults.horizontalBar.scales.yAxes[0].categoryPercentage = 0.5;
        ChartJsProvider.$get().Chart.defaults.line.scales.yAxes[0].ticks = { callback: lineChartCallback }; // fix for decimal values
        ChartJsProvider.$get().Chart.defaults.scale.ticks.padding = 5; // fix for label getting cut off on y-axis

        function lineChartCallback(value) {
            if (!(value % 1)) {
                return Number(value).toFixed(0);
            }
        }

        cfpLoadingBarProvider.includeSpinner = false;
        cfpLoadingBarProvider.latencyThreshold = 0;

        if (!$httpProvider.defaults.headers.get) {
            $httpProvider.defaults.headers.get = {};
        }
        //disable IE ajax request caching
        $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
        $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
        $httpProvider.defaults.headers.common['Expires'] = '0';

        // Redirect to error page
        var errorInterceptor = ['$rootScope', '$q', function($rootScope, $q) {
            function success(response) {
                return response;
            }

            function error(response) {

                if (response.config.bypassErrors && response.config.bypassErrors.contains(response.status)) {
                    return $q.reject(response);
                }

                //if you want to exclude more errors you need to put status code in this if statement
                if (response.status !== 400 &&
                    response.status !== 401 &&
                    response.status !== 403 &&
                    response.status !== 419 &&
                    response.status !== 440 &&
                    response.status !== 404) {
                    $rootScope.$broadcast('generic-error', response.status);
                }

                return $q.reject(response);
            }

            return function(promise) {
                return promise.then(success, error);
            };
        }];

        $httpProvider.interceptors.push(errorInterceptor);
    }

    localesTranslations.$inject = ['$translateProvider', 'LOCALES', 'showMissingTranslations'];

    function localesTranslations($translateProvider, LOCALES, showMissingTranslations) {
        if (showMissingTranslations) {
            $translateProvider.useMissingTranslationHandler('missingTranslationHandler');
        }

        $translateProvider.useSanitizeValueStrategy('');
        $translateProvider.useStaticFilesLoader({
            prefix: '/resources/locale-',
            suffix: '.json'
        });

        $translateProvider.preferredLanguage(LOCALES.preferredLocale);
        $translateProvider.useLocalStorage();
    }

    dynamicLocale.$inject = ['tmhDynamicLocaleProvider'];

    function dynamicLocale(tmhDynamicLocaleProvider) {
        tmhDynamicLocaleProvider.localeLocationPattern('resources/angular-locale_{{locale}}.js');
    }
})();
