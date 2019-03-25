(function () {
    'use strict';

    angular.module('simoonaApp.Role', ['ui.router'])
        .config(route);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider

            // Roles
            .state('Root.WithOrg.Admin.Roles', {
                abstract: true,
                url: '/Roles',
                template: '<ui-view></ui-view>',
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROLES_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Roles.List', {
                url: '?sort&dir&page&s',
                templateUrl: 'app/role/role-list.html',
                controller: 'rolesListController',
                reloadOnSearch: false,
                resolve: {
                    roles: [
                        '$stateParams', 'roleRepository', function ($stateParams, roleRepository) {
                            return roleRepository.getPaged($stateParams);
                        }
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROLES_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Roles.Edit', {
                url: '/:roleId/Edit',
                templateUrl: 'app/role/role-manage.html',
                controller: 'rolesManageController',
                reloadOnSearch: false,
                resolve: {
                    role: [
                        '$stateParams', 'roleRepository', function ($stateParams, roleRepository) {
                            return roleRepository.get({ roleId : $stateParams.roleId });
                        },
                    ],
                    controllers: function () {
                        return undefined;
                    },
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROLES_ADMINISTRATION'
                }
            })
            .state('Root.WithOrg.Admin.Roles.Create', {
                url: '/Create',
                templateUrl: 'app/role/role-manage.html',
                controller: 'rolesManageController',
                reloadOnSearch: false,
                resolve: {
                    role: function () {
                        return {};
                    },
                    controllers: [
                        'roleRepository', function (roleRepository) {
                            return roleRepository.GetPermissionGroups();
                        },
                    ]
                },
                data: {
                    authorizeRole: 'Admin',
                    authorizePermission: 'ROLES_ADMINISTRATION'
                }
            });
    }
})();
