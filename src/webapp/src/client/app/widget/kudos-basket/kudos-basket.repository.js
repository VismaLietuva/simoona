(function () {
    'use strict';

    angular
        .module('simoonaApp.Widget.KudosBasket')
        .factory('kudosBasketRepository', kudosBasketRepository);

    kudosBasketRepository.$inject = ['$resource', 'endPoint'];

    function kudosBasketRepository($resource, endPoint) {
        var url = endPoint + '/KudosBasket/';

        var service = {
            getDonations: getDonations,
            makeDonation: makeDonation,
            getKudosBasket: getKudosBasket,
            getKudosBasketWidget: getKudosBasketWidget,
            createNewBasket: createNewBasket,
            editKudosBasket: editKudosBasket,
            deleteKudosBasket: deleteKudosBasket
        };
        return service;

        /////

        function getDonations() {
            return $resource(url + 'GetDonations', '', {
                get: {
                    method: 'GET',
                    isArray: true
                }
            }).get().$promise;
        }

        function makeDonation(kudosBasket, amount) {
            return $resource(url + 'MakeDonation', '', {
                post: {
                    method: 'POST'
                }
            }).post({
                id: kudosBasket.id,
                DonationAmount: amount
            }).$promise;
        }

        function getKudosBasket() {
            return $resource(url + 'GetKudosBasket').get().$promise;
        }

        function getKudosBasketWidget() {
            return $resource(url + 'GetKudosBasketWidget').get().$promise;
        }

        function createNewBasket(params) {
            return $resource(url + 'CreateNewKudosBasket', '', {
                post: {
                    method: 'POST'
                }
            }).post(params).$promise;
        }

        function editKudosBasket(params) {
            return $resource(url + 'EditKudosBasket', '', {
                put: {
                    method: 'PUT'
                }
            }).put(params).$promise;
        }

        function deleteKudosBasket() {
            return $resource(url + 'DeleteKudosBasket', {}, {
                delete: {
                    method: 'DELETE',
                    bypassErrors: [404]
                }
            }).delete().$promise;
        }
    }
}());
