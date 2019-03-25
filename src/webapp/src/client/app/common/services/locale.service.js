(function() {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('localeSrv', localeSrv);

    localeSrv.$inject = [
        '$translate',
        'LOCALES',
        '$rootScope',
        'tmhDynamicLocale',
        'amMoment',
        'lodash'
    ];

    function localeSrv($translate, LOCALES, $rootScope, tmhDynamicLocale, amMoment, lodash) {
        var localesObj = LOCALES.locales;
        var _LOCALES = Object.keys(localesObj);
        if (!_LOCALES || _LOCALES.length === 0) {
            console.error('There are no _LOCALES provided');
        }

        var _LOCALES_DISPLAY_NAMES = [];
        _LOCALES.forEach(function(locale) {
            _LOCALES_DISPLAY_NAMES.push(localesObj[locale]);
        });

        var currentLocale = $translate.proposedLanguage() || $translate.use();

        var service = {
            getLocaleDisplayName: getLocaleDisplayName,
            setLocaleByDisplayName: setLocaleByDisplayName,
            getLocalesDisplayNames: getLocalesDisplayNames,
            formatTranslation: formatTranslation,
            getLocales: getLocales,
            setLocale: setLocale,
            translate: translate
        };

        init();

        return service;

        //////

        function init() {
            $rootScope.$on('$translateChangeSuccess', function(event, data) {
                document.documentElement.setAttribute('lang', data.language); // sets "lang" attribute to html
                tmhDynamicLocale.set(data.language.toLowerCase().replace(/_/g, '-')); // load Angular locale
            });

            setLocale(currentLocale);
        }

        // METHODS
        function isLocaleValid(locale) {
            return _LOCALES.indexOf(locale) !== -1;
        }

        function setLocale(locale) {
            if(!locale) {
                return;
            }

            locale = lodash.replace(locale, '-', '_');

            if (!isLocaleValid(locale)) {
                console.error('Locale name "' + locale + '" is invalid');
                return;
            }

            currentLocale = locale;
            if (currentLocale === 'lt_LT') {
                amMoment.changeLocale('lt');
            } else if (currentLocale === 'en_US') {
                amMoment.changeLocale('en');
            }

            $translate.use(locale);
        }

        function getLocaleDisplayName() {
            return localesObj[currentLocale];
        }

        function setLocaleByDisplayName(localeDisplayName) {
            setLocale(
                _LOCALES[
                    _LOCALES_DISPLAY_NAMES.indexOf(localeDisplayName) // get locale index
                ]
            );
        }

        function getLocalesDisplayNames() {
            return _LOCALES_DISPLAY_NAMES;
        }

        function getLocales() {
            var localesList = [];
            lodash.forEach(_LOCALES, function(value, index) {
                localesList.push({
                    locale: value,
                    displayName: _LOCALES_DISPLAY_NAMES[index]
                });
            });

            return localesList;
        }

        function formatTranslation(format, parameters) {
            if (format) {
                for (var prop in parameters) {
                    if (parameters.hasOwnProperty(prop) && angular.isString(parameters[prop])) {
                        parameters[prop] = $translate.instant(parameters[prop]);
                    }
                }

                return $translate.instant(format, parameters);
            } else {
                return 'undefined string';
            }
        }

        function translate(resource) {
            return $translate.instant(resource);
        }
    }
})();
