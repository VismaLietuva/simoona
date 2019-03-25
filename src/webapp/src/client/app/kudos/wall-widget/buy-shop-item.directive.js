(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .directive('buyShopItem', buyShopItem);

    buyShopItem.$inject = [
        'serviceRequestRepository'
    ];

    function buyShopItem(serviceRequestRepository) {
        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'app/kudos/wall-widget/buy-shop-item.html',
            link: linkFunc
        };
        return directive;

        function linkFunc(scope) {
            scope.hasKudosShopItems = false;

            init();

            ////////

            function init() {
                hasKudosShopItems();
            }

            function hasKudosShopItems() {
                serviceRequestRepository.shopItemsExist().then(function (response) {
                    scope.hasKudosShopItems = response;
                });
            }
        }
    }
})();