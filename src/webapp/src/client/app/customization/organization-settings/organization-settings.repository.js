(function() {
    'use strict';

    angular
        .module('simoonaApp.Customization.OrganizationSettings')
        .factory('organizationSettingsRepository', organizationSettingsRepository);

    organizationSettingsRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function organizationSettingsRepository($resource, endPoint) {
        var organizationSettingsUrl = endPoint + '/Organization/';
        var applicationUserUrl = endPoint + '/ApplicationUser/';

        var service = {
            getOrganizationSettings: getOrganizationSettings,
            postOrganizationSettings: postOrganizationSettings,
            getManagersForAutocomplete: getManagersForAutocomplete
        };
        return service;

        /////////

        function getOrganizationSettings() {
            return $resource(organizationSettingsUrl + 'getManagingDirector').get().$promise;
        }

        function postOrganizationSettings(manager) {
            return $resource(organizationSettingsUrl + 'setManagingDirector', { 
                userId: manager.id 
            }).save().$promise;
        }

        function getManagersForAutocomplete(searchString) {
            return $resource(applicationUserUrl + 'GetManagersForAutocomplete', '', {
                'query': {
                    method: 'GET',
                    isArray: true
                }
            }).query({
                s: searchString
            }).$promise;
        }

    }
})();
