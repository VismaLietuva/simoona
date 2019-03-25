'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Map');

    simoonaApp.factory('pinService', [
        '$timeout', '$location', 'zoom', '$advancedLocation',
        function($timeout, $location, zoom, $advancedLocation) {
            var scope;
            return {
                watchPin: function() {
                    var self = this;
                    if (!scope.enablePin) return;

                    scope.$watch(
                        function () { return scope.floor.pinCoords; },
                        function(newCoords) {
                            if (!newCoords) return;
                            newCoords.x = parseInt(newCoords.x);
                            newCoords.y = parseInt(newCoords.y);
                            self.placePin(newCoords);
                        });
                },
                placePin: function(coords) {
                    var self = this;
                    self.removePin();
                    if (scope.enablePin) {
                        var path = "M 13.619988,24.7411 C 12.127098,22.1556 10.199648,18.573 9.0752378,16.923 4.8986677,9.3217 8.5794978,2.6951 16.106538,2.6194 c 7.45828,0 10.84789,7.4035 6.91158,14.261 -1.27626,2.2234 -3.35037,5.9857 -4.60913,8.3607 l -2.11009,4.1395 z m 6.92608,-9.1517 c 1.00581,-0.9102 1.4716,-2.5316 1.4716,-3.8214 0,-2.8699 -2.21798,-6.3009 -6,-6.345 -3.63218,-0.042 -6,3.4498 -6,6.1713 0,5.0339 6.70127,7.4586 10.5284,3.9951 z";
                        var pBBox = Raphael.pathBBox(path);
                        var matrix = Raphael.matrix(1, 0, 0, 1, coords.x - pBBox.width, coords.y - pBBox.height);
                        var pString = Raphael.mapPath(path, matrix);
                        scope.pin = scope.paper.path(pString.toString());
                        scope.pin.attr({ fill: '#063fef', 'stroke-width': '0' });
                        scope.pin.toFront();
                        zoom.addUnzoomablePath(scope.pin);
                    }
                },
                removePin: function(coords) {
                    if (scope.pin) {
                        scope.pin.remove();
                        zoom.removeUnzoomablePath(scope.pin);
                    }
                    scope.pin = null;
                },
                init: function($scope) {
                    var self = this;
                    scope = $scope;

                    scope.svg.on('mousedown', function(e) {
                        var params = $location.search();

                        if (e.which !== 3) return;
                        var offset = scope.container.offset();
                        var coords = {
                            x: e.pageX - parseInt(offset.left),
                            y: e.pageY - parseInt(offset.top)
                        }

                        self.placePin(coords);
                        coords = zoom.getCoordsRelativeToScreen(coords.x, coords.y);


                        params.floorId = scope.floor.id;

                        if (!scope.clickedRoom && scope.selectedRoom) {
                            delete params.roomId;
                        }

                        delete params.coords;
                        params.coords = coords.x + ',' + coords.y;
                        $advancedLocation.search(params);
                        scope.$apply();

                        e.preventDefault();
                    }).click(function(e) {
                        if (zoom.panning) return;
                        var params = $location.search();

                        self.removePin();
                        delete params.coords;
                        if (!scope.clickedRoom) {
                            delete params.roomId;
                        }
                        $advancedLocation.search(params);
                        scope.$apply();
                    });
                }
            };
        }
    ]);
})();