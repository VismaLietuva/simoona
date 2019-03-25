'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Common');

    simoonaApp.factory('$advancedLocation', [
        '$location', '$state', function($location, $state) {
            var location = angular.copy($location);

            function AdvancedLocation() {
            };

            AdvancedLocation.prototype = location;

            function filterNullParams(params) {
                for (var prop in params) {
                    if (params.hasOwnProperty(prop) && !params[prop]) {
                        delete params[prop];
                    }
                }
            }

            function updateState(params) {
                for (var prop in params) {
                    $state.params[prop] = params[prop];
                }
            }

            AdvancedLocation.prototype.search = function() {
                filterNullParams.call(null, arguments[0]);
                updateState(arguments[0]);
                $location.search(arguments[0]);
            };

            var o = new AdvancedLocation();
            return o;
        }
    ]);
})();