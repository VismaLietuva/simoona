(function () {
    'use strict';

    angular
        .module('simoonaApp.Profile')
        .controller('deleteExamCtrl', deleteExamCtrl);

    deleteExamCtrl.$inject = ['$scope', '$uibModalInstance', '$translate'];

    function deleteExamCtrl($scope, $uibModalInstance, $translate) {

        
        $scope.save = save;
        $scope.cancel = cancel;

        function save(certificate) {
            $uibModalInstance.close();
        }

        function cancel() {
            $uibModalInstance.dismiss();
        }
    }
})();
