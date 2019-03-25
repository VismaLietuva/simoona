(function() {
    'use strict';

    angular.module('simoonaApp.Role')
        .controller('rolesManageController', rolesManageController);

    rolesManageController.$inject = ['$scope', '$state', 'role', 'roleRepository', '$rootScope', 'authService',
        '$translate', 'notifySrv', 'controllers', '$q', 'localeSrv', 'errorHandler'
    ];

    function rolesManageController($scope, $state, role, roleRepository, $rootScope, authService,
        $translate, notifySrv, controllers, $q, localeSrv, errorHandler) {

        $scope.onSave = onSave;
        $scope.loadUsers = loadUsers;

        if (controllers) {
            $scope.permissionGroups = controllers;
        }

        if (role.permissions) {
            $scope.permissionGroups = role.permissions;
        }

        var states = {
            isAdd: $state.includes('Root.WithOrg.Admin.Roles.Create'),
            isEdit: $state.includes('Root.WithOrg.Admin.Roles.Edit')
        };

        if (states.isEdit) {
            setTitleScope(true, false, 'role.editRole');
        } else if (states.isAdd) {
            setTitleScope(false, true, 'role.createRole');
        }

        $scope.role = role;

        function setTitleScope(titleEdit, titleCreate, pageTitle) {
            $scope.titleEdit = titleEdit;
            $scope.titleCreate = titleCreate;
            $rootScope.pageTitle = pageTitle;
        }

        function onSave() {
            $scope.errors = [];

            if ($state.current.name === 'Root.WithOrg.Admin.Roles.Edit') {
                roleRepository.update($scope.role).then(function() {
                        notifySrv.success(localeSrv.formatTranslation('common.messageEntityChanged', { one: 'role.entityName', two: $scope.role.name }));

                        var deferred = $q.defer();
                    }, errorHandler.handleErrorMessage);
            }
            
            if ($state.current.name === 'Root.WithOrg.Admin.Roles.Create') {
                $scope.role.permissions = $scope.permissionGroups;
                roleRepository.create($scope.role).then(function() {
                        notifySrv.success(localeSrv.formatTranslation('common.messageEntityCreated', { one: 'role.entityName', two: $scope.role.name }));
                        $state.go('^.List');
                    }, errorHandler.handleErrorMessage);
            }
        }

        function loadUsers(search) {
            return roleRepository.getUsersForAutoComplete(search);
        }
    };
})();
