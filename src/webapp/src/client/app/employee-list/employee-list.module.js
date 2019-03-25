(function () {
    'use strict';
    window.modules = window.modules || [];
    window.modules.push('simoonaApp.EmployeeList');
    angular
        .module('simoonaApp.EmployeeList', [
            'ui.router',
            'simoonaApp.Common'
        ])
        .config(route)
        .run(init);

    route.$inject = ['$stateProvider'];

    function route($stateProvider) {
        $stateProvider
            .state('Root.WithOrg.Client.Employee', {
                abstract: true,
                url: '/Employee',
                template: '<ui-view></ui-view>'
            })
            .state('Root.WithOrg.Client.Employee.List', {
                url: '/List',
                //reloadOnSearch: false,
                controller: 'EmployeeListController',
                templateUrl: 'app/employee-list/employee-list.html',
                resolve: {
                    employeeList: [
                        'employeeListRepository',
                        function (employeeListRepository) {
                            return employeeListRepository.getPaged({});
                        }
                    ]
                }
            });
    }

    init.$inject = [
        'menuNavigationFactory',
        'leftMenuGroups'
    ];

    function init(menuNavigationFactory, leftMenuGroups) {
        menuNavigationFactory.defineLeftMenuItem({
            permission: 'EMPLOYEELIST_BASIC',
            url: 'Root.WithOrg.Client.Employee.List',
            active: 'Root.WithOrg.Client.Employee.List',
            resource: 'navbar.employees',
            order: 1,
            group: leftMenuGroups.company
        });
    }
})();
