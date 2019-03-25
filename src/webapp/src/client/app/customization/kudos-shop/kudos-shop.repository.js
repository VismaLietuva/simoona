(function () {
    'use strict';

    angular
        .module('simoonaApp.Customization.KudosShop')
        .factory('kudosShopRepository', kudosShopRepository);

    kudosShopRepository.$inject = [
        '$resource',
        'endPoint'
    ];

    function kudosShopRepository($resource, endPoint) {
        var kudosShopUrl = endPoint + '/KudosShop/';

        var service = {
            getKudosShopItems: getKudosShopItems,
            getKudosShopItem: getKudosShopItem,
            createKudosShopItem: createKudosShopItem,
            updateKudosShopItem: updateKudosShopItem,
            deleteKudosShopItem: deleteKudosShopItem
        };
        return service;

        ///////////

        function getKudosShopItems() {
            return $resource(kudosShopUrl + 'All').query().$promise;
        }

        function getKudosShopItem(id) {
            return $resource(kudosShopUrl + 'Get').get({ id: id }).$promise;
        }

        function createKudosShopItem(kudosItem) {
            return $resource(kudosShopUrl + 'Create').save(kudosItem).$promise;
        }

        function updateKudosShopItem(kudosItem) {
            return $resource(kudosShopUrl + 'Update', '', {
                put: {
                    method: 'PUT'
                }
            }).put(kudosItem).$promise;
        }

        function deleteKudosShopItem(id) {
            return $resource(kudosShopUrl + 'Delete').delete({ id: id }).$promise;
        }
    }
})();