(function() {

    'use strict';

    angular
        .module('simoonaApp.Common')
        .factory('chartDataFactory', chartDataFactory);

    chartDataFactory.$inject = ['charactersFilter'];

    function chartDataFactory(charactersFilter) {

        var service = {
            formatPieChartData: formatPieChartData,
            formatHorizontalChartData: formatHorizontalChartData,
            formatColumnChartData: formatColumnChartData
        }

        return service;

        function formatPieChartData(array, label, data) {
            var obj = {
                labels: [],
                data: []
            };
            for (var i = 0; array.length > i; i++) {
                obj.data.push(array[i][data]);
                obj.labels.push(array[i][label]);
            }

            return obj;
        }

        function formatHorizontalChartData(array, label, data) {
            var obj = {
                labels: [],
                data: []
            };
            for (var i = 0; array.length > i; i++) {
                obj.data.push(array[i][data]);
                obj.labels.push(charactersFilter(array[i][label], 20, true));
            }

            obj.data = [obj.data];
            return obj;
        }

        function formatColumnChartData(array, label, data, series) {
            var diff;
            var seriesIndex;
            var obj = {
                labels: [],
                data: [],
                series: []
            };

            array.forEach(function(item) { // loop through array
                if (obj.labels.indexOf(item[label]) < 0) { // get unique labels (if label isn't already in array, its' index is -1) 
                    obj.labels.push(item[label]); // push them to array
                }

                if (obj.series.indexOf(item[series]) < 0) { // get unique series 
                    obj.series.push(item[series]); // push them to array
                    obj.data.push([item[data]]); // if new series, also push people count to new array
                } else {
                    obj.series.forEach(function(serie) { // if series it not unique 
                        if (serie === item[series]) { // find that serie
                            return seriesIndex = obj.series.indexOf(serie); //get get its' index
                        }
                    });
                    obj.data[seriesIndex].push(item[data]); // and push people count to that particular array 
                }
            });

            obj.data.forEach(function(item) { // loop through array
                diff = obj.series.length - item.length; // get number of missing 0 
                if (diff) {
                    for (var i = 0; i < diff; i++) {
                        item.unshift(0); // and add them to beginning of an array
                    }
                }
            });
            if (obj.data.length > 0) {
                return obj;
            } else {
                return false;
            }

        }
    }
})();
