(function () {
    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('walkThroughService', walkThroughService);

    function walkThroughService(authService, $window, $translate) {

        var service = {
            startWalkThrough: startWalkThrough
        };

        return service;

        ///////////////

        function startWalkThrough() {
            var tour = new Tour({
                steps: [
                    {
                        title: $translate.instant('walkthrough.welcome'),
                        content: $translate.instant('walkthrough.welcomeDescription'),
                        container: "body",
                        backdrop: true
                    },
                    {
                        element: "#walls-sidebar-nav",
                        title: $translate.instant('walkthrough.walls'),
                        content: $translate.instant('walkthrough.wallsDescription'),
                        placement: "right",
                        prev: -1
                    },
                    {
                        element: "#activities-sidebar-nav",
                        title: $translate.instant('walkthrough.activities'),
                        content: $translate.instant('walkthrough.activitiesDescription'),
                        placement: "right"
                    },
                    {
                        element: "#company-sidebar-nav",
                        title: $translate.instant('walkthrough.company'),
                        content: $translate.instant('walkthrough.companyDescription'),
                        placement: "right"
                    },
                    {
                        element: "#user-nav-btn",
                        title: $translate.instant('walkthrough.profile'),
                        content: $translate.instant('walkthrough.profileDescription'),
                        placement: "bottom"
                    },
                    {
                        element: "#administration-nav-btn",
                        title: $translate.instant('walkthrough.administration'),
                        content: $translate.instant('walkthrough.administrationDescription'),
                        placement: "bottom"
                    },
                    {
                        element: "#kudos-wall-widget",
                        title: $translate.instant('walkthrough.kudos'),
                        content: $translate.instant('walkthrough.kudosDescription'),
                        placement: "left"
                    }
                ],
                orphan: true,
                template: "<div class='popover tour walkthrough-popover'> <div class='arrow'></div><button data-role='end' type='button' class='close' aria-label='Close'><span aria-hidden='true'>&times;</span></button> <h3 class='popover-title'></h3> <div class='popover-content'></div> <div class='popover-navigation'><div class='btn-group'><button class='btn btn-sm walkthrough-btn' data-role='prev'>" + $translate.instant('walkthrough.prev') + "</button><button class='btn btn-sm walkthrough-btn' data-role='next'>" + $translate.instant('walkthrough.next') + "</button></div><button class='btn btn-sm walkthrough-btn' data-role='end'>" + $translate.instant('walkthrough.done') + "</button></div></div>",
                onEnd: function (tour) {
                    authService.completeWalkThrough();
                }
            });

            authService.walkThroughCompletionStatus().then(function (response) {
                var hasCompletedWalkThrough = response.data;
                if (!hasCompletedWalkThrough && $window.innerWidth > 991) {
                    tour.init();
                    tour.setCurrentStep(0);
                    tour.start(true);
                }
            });

        }
    }

})();
