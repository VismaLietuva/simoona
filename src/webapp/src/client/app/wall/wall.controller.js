(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .controller('wallController', wallController);

    wallController.$inject = [
        '$rootScope',
        'wallRepository'
    ];

    function wallController($rootScope, wallRepository) {
        /*jshint validthis: true */
        var vm = this;

                //TEMPORARY COUNTER
                const second = 1000,
                minute = second * 60,
                hour = minute * 60,
                day = hour * 24;
        
                var gotStarts = new Date("Dec 9, 2022 18:00:00").getTime();
                
                const countDown = new Date(gotStarts).getTime();
                const x = setInterval(function() {    
        
                        const now = new Date().getTime(),
                            distance = countDown - now;
        
                        if (distance < 0) {
                            document.getElementById("heading").innerText = "Game of Thrones starts now!";
                            document.getElementById("counting-sm").style.display = document.getElementById("counting-lg").style.display  = 'none';
                            
                            clearInterval(x);
                        } else {
                            document.getElementById("days-sm").innerText = document.getElementById("days-lg").innerText = Math.floor(distance / (day)),
                            document.getElementById("hours-sm").innerText = document.getElementById("hours-lg").innerText = Math.floor((distance % (day)) / (hour)),
                            document.getElementById("minutes-sm").innerText = document.getElementById("minutes-lg").innerText = Math.floor((distance % (hour)) / (minute)),
                            document.getElementById("seconds-sm").innerText = document.getElementById("seconds-lg").innerText = Math.floor((distance % (minute)) / second);
                        }
                        //seconds
                    }, 0)
                //TEMPORARY COUNTER

        $rootScope.pageTitle = 'wall.wallTitle';

        //init
        vm.widgetsInfo = {};

        wallRepository.getWidgetsInfo()
            .then(function(widgetsInfo) { 
                vm.widgetsInfo = widgetsInfo;
            }); 
    }
}());
