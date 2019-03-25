(function() {
    'use strict';

    angular
        .module('simoonaApp.Project')
        .component('aceProjectDescription', {
            bindings: {
                project: '=',
                isLoading: '='
            },
            templateUrl: 'app/project/content/description/description.html',
            controller: projectDescriptionController,
            controllerAs: 'vm'
        });

    projectDescriptionController.$inject = [
        'projectSettings'
    ];

    function projectDescriptionController(projectSettings) {
        /* jshint validthis: true */
        var vm = this;

        vm.projectImageSize = {
            w: projectSettings.thumbWidth,
            h: projectSettings.thumbHeight
        };
    }
})();
