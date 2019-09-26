(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .factory('kudosifyModalFactory', kudosifyModalFactory);

    kudosifyModalFactory.$inject = ['$resource', 'endPoint'];

    function kudosifyModalFactory($resource, endPoint) {
        var url = endPoint + '/Kudos/';

        var service = {
            postKudos: postKudos,
            getPointsTypes: getPointsTypes,
            getSendType: getSendType
        };
        return service;

        /////

        function postKudos(kudosReceivers, kudosifyInfo, pointsType) {
            return $resource(url + 'AddKudosLog').save({
                ReceivingUserIds: kudosReceivers,
                MultiplyBy: kudosifyInfo.multiplyBy,
                Comment: kudosifyInfo.comment,
                TotalPointsPerReceiver: kudosifyInfo.totalPoints,
                PointsTypeId: pointsType.id,
                PictureId: kudosifyInfo.imageName
            }).$promise;
        }

        function getPointsTypes() {
            return $resource(url + 'GetKudosTypes').query().$promise;
        }

        function getSendType() {
            return $resource(url + 'GetSendType').get().$promise;
        }
    }
})();
