(function () {
    "use strict";

    angular.module('simoonaApp.Role')
        .controller('rolesListController', rolesListController);

    rolesListController.$inject = ['$scope', '$rootScope', '$state', 'roles', '$advancedLocation', 'roleRepository', '$uibModal',
     'authService', '$translate', 'notifySrv', '$timeout', 'localeSrv'];

    function rolesListController($scope, $rootScope, $state, roles, $advancedLocation, roleRepository, $uibModal,
     authService, $translate, notifySrv, $timeout, localeSrv) {

        $rootScope.pageTitle = 'role.entityNamePlural';

        $scope.roles = roles;

        $scope.allowEdit = authService.hasPermissions(['ROLES_ADMINISTRATION']);
        
        $scope.getItems = function () {
            var parameters = {};

            if ($scope.filter.sort !== null) {
                parameters.sort = $scope.filter.sort;
                parameters.dir = $scope.filter.dir;
            };

            if ($scope.filter.page !== null) {
                parameters.page = $scope.filter.page;
            };

            if ($scope.filter.s !== null && $scope.filter.s !== '' && $scope.filter.s !== undefined) {
                parameters.s = $scope.filter.s;
            }

            roleRepository.getPaged(parameters).then(function (roles) {
                $scope.roles = roles;
            }, function () {});
            $advancedLocation.search(parameters);
        };


        $scope.filter = {
            page: $state.params.page,
            dir: $state.params.dir,
            sort: $state.params.sort,
            s: $state.params.s
        };

        $scope.onSearch = function (search) {
            $scope.filter.s = search;
            $scope.filter.page = 1;
            $scope.getItems();
        };

        $scope.onSort = function (sort, dir) {
            $scope.filter.dir = dir;
            $scope.filter.sort = sort;
            $scope.filter.page = 1;
            $scope.getItems();
        };

        $scope.deleteItem = function (role) {
            roleRepository.deleteItem(role.id).then(function () {
                notifySrv.success(localeSrv.formatTranslation('common.messageEntityDeleted', {one: 'role.entityName', two: role.name}));
                if ($scope.roles.pagedList.length === 1) {
                    $scope.filter.s = null;
                }
                $scope.getItems();
            });
        }

    };
})();
