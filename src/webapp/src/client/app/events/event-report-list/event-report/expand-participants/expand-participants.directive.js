(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .directive('aceExpandParticipants', aceExpandParticipants);

    function aceExpandParticipants() {
        var directive = {
            templateUrl:
                'app/events/event-report-list/event-report/expand-participants/expand-participants.html',
            restrict: 'E',
            replace: true,
            scope: {
                eventTypes: '=',
                userId: '=',
                startDate: '=',
                endDate: '='
            },
            controller: expandParticipantsController,
            controllerAs: 'vm',
            bindToController: true,
        };

        return directive;
    }

    expandParticipantsController.$inject = [
        'eventRepository',
        'notifySrv',
        'sortMultipleLinkService'
    ];

    function expandParticipantsController(eventRepository, notifySrv, sortMultipleLinkService) {
        var vm = this;

        var columnCount = 4;

        vm.page = 1;

        vm.sortValues = sortMultipleLinkService.getMultipleSort(columnCount);

        vm.loadVisitedEventsOnPage = loadVisitedEventsOnPage;
        vm.sortByColumn = sortByColumn;

        init();

        function init() {
            loadVisitedEvents();
        }

        function loadVisitedEvents() {
            vm.isLoading = true;

            eventRepository
                .getEventParticipantVisitedEvents(
                    vm.userId,
                    vm.page,
                    vm.sortValues.getSortString(),
                    vm.eventTypes,
                    vm.startDate,
                    vm.endDate
                )
                .then(
                    function (response) {
                        vm.isLoading = false;
                        vm.events = response;
                    },
                    function () {
                        notifySrv.error('errorCodeMessages.messageError');
                    }
                );
        }

        function sortByColumn(sortBy, sortOrder, position) {
            onCompleteLoadFirstPage(function() {
                vm.sortValues.setSortValues(sortBy, sortOrder, position);
            });
        }

        function onCompleteLoadFirstPage(func) {
            func();
            loadVisitedEventsOnPage(1);
        }

        function loadVisitedEventsOnPage(page) {
            vm.page = page;
            loadVisitedEvents();
        }
    }
})();
