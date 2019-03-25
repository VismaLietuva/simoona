(function () {
    'use strict';

    angular
        .module('simoonaApp.Kudos')
        .component('aceKudosCommitteeWidget', {
            templateUrl: 'app/kudos/committee-widget/committee-widget.html',
            controller: kudosCommitteeController,
            controllerAs: 'vm'
        });

    kudosCommitteeController.$inject = [
        'committeeRepository'
    ];

    function kudosCommitteeController(committeeRepository) {
        /*jshint validthis: true */
        var vm = this;

        init();

        //////////

        function init() {
            committeeRepository.getKudosCommittee().then(function (response) {
                vm.kudosCommittee = response;
            });
        }
    }
})();
