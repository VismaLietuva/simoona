(function() {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .component('aceCalendarBtn', {
            bindings: {
                calendarItem: '='
            },
            templateUrl: 'app/events/calendar/calendar-btn.html',
            controller: calendarButtonController,
            controllerAs: 'vm'
        })
        .constant('googleCalendarBaseUrl', 'https://calendar.google.com/calendar/r/eventedit?');

    calendarButtonController.$inject = [
            'googleCalendarBaseUrl',
            'eventRepository',
            '$location'
        ];

    function calendarButtonController(googleCalendarBaseUrl, eventRepository, $location) {
        /*jshint validthis: true */
        var vm = this;

        vm.downloadEvent = downloadEvent;

        setupRedirectLinks();

        function setupRedirectLinks() {
            var googleCalendarStartDate = vm.calendarItem.startDate.replace(/-|:|\.\d\d\d/g,"");
            var googleCalendarEndDate = vm.calendarItem.endDate.replace(/-|:|\.\d\d\d/g,"");

            var title = encodeURIComponent(vm.calendarItem.name);
            var details = encodeURIComponent(`${vm.calendarItem.description}\n\n${$location.absUrl()}`);
            var location = encodeURIComponent(vm.calendarItem.location);
            
            vm.googleCalendarRedirect = `${googleCalendarBaseUrl}&text=${title}&location=${location}&dates=${googleCalendarStartDate}Z/${googleCalendarEndDate}Z&details=${details}`;    
        }

        function downloadEvent() {
            eventRepository.downloadEvent(vm.calendarItem.id)
            .then(function(response){
                var file = new Blob([response.data], {
                    type: 'text/calendar;'
                });
                var dateFormat = new Date(vm.calendarItem.startDate).toISOString().split('T')[0];
                var fileName = `${vm.calendarItem.name.replace(/\s/g, '')}_${dateFormat}`;

                saveAs(file,fileName);
            }, function (error) {
                errorHandler.handleErrorMessage(error);
            });
        }
    
    }
}());
