(function () {
    'use strict';

    angular
        .module('simoonaApp.Map')
        .controller('mapController', mapController);

    mapController.$inject = [
        '$rootScope',
        '$scope',
        '$timeout',
        'roomRepository',
        'userRepository',
        'pinService',
        'roomManageService',
        'zoom',
        '$filter'
    ];

    function mapController($rootScope, $scope, $timeout, roomRepository,
        userRepository, pinService, roomManageService, zoom, $filter) {

        var self = this;
        var icon = {};
        var handDrawingEnabled = false;
        $scope.redrawMode = false;
        $scope.model = {};

        //Constants
        this.mouseoverOpacity = 0.4;
        this.opacity = 0;
        this.clickOpacity = 0.4;
        this.margin = 0;
        this.roomFillColor = '#8398af';

        $scope.iconImageOnLoad = function (width, height, room) {
            room.roomType.icon = {
                thumWidth: width,
                thumHeight: height
            };

            drawRoomIcon(room);
        };

        $scope.drawRoom = function(room) {
            var coords = $scope.parseCoordinates(room.coordinates);
            var path = 'M';
            for (var i = 0; i < coords.length; i++) {
                if ((i + 1) % 2 === 0) {
                    path += (coords[i - 1] * $scope.aspectRatio + self.margin) + ' ' + (coords[i] * $scope.aspectRatio + self.margin) + 'L';
                }
            }

            path = path.slice(0, -1);
            path += 'Z';
            if (!$scope.redrawMode) {
                $scope.arrayOfPaths.push(path);
                $scope.arrayOfRooms.push(room);
            }

            if (room.roomType === undefined || room.roomType === null) {
                room.roomType = {};
                room.roomType.color = self.roomFillColor;
            }

            room.drawing = $scope.paper.path(path)
                .attr({
                    'fill': room.roomType.color,
                    'opacity': self.opacity,
                    'stroke-width': 0
                })
                .mouseover(function() {
                    if (!room.selected && room.drawing) {
                        room.drawing.attr({
                            opacity: self.mouseoverOpacity
                        });
                    }

                    $scope.container.removeClass('cursor-inherit').addClass('pointer');
                })
                .mouseout(function() {
                    if (!room.selected && room.drawing) {
                        room.drawing.attr({
                            opacity: self.opacity
                        });
                    }

                    $scope.container.removeClass('pointer').addClass('cursor-inherit');
                })
                .mouseup(function(e) {
                    if (e.which !== 1) {
                        return;
                    }

                    $scope.clickedRoom = room;
                });

            drawRoomIcon(room);

            if (room.selected) {
                room.drawing.attr({
                    opacity: 1,
                    fill: 'none',
                    stroke: '#000000',
                    'stroke-width': 2
                });
                $scope.selectedRoom = room;
            }
        };

        function drawRoomIcon(room) {
            //TODO, office picture should be uploaded before roomType icon, need to solve this problem via promises
            if ($scope.floor.picture && room.roomType && room.roomType.icon && room.roomType.iconId)
            {
                var coords = $scope.parseCoordinates(room.coordinates);
                var center = $scope.getPolygonCenter(coords);
                var maxWidth, maxHeight = 24;

                var size = $scope.calculateIconSize(room.roomType.icon.thumbWidth, room.roomType.icon.thumbHeight, maxWidth, maxHeight);

                room.mark = $scope.paper.image($filter('thumb')(room.roomType.iconId),
                    center.x + self.margin - size.width / 2,
                    center.y + self.margin - size.height / 2,
                    size.width, size.height).toFront();
                room.mark.node.setAttribute('style', 'pointer-events: none');
                zoom.addUnzoomablePath(room.mark);
            }
        }

        $scope.calculateIconSize = function(originalWidth, originalHeight, requiredWidth, requiredHeight) {
            var size = {
                width: requiredWidth,
                height: requiredHeight
            };

            var ratio = originalWidth / originalHeight;

            if (ratio > 1) {
                size.height = requiredWidth * (1 / ratio);
            } else {
                size.width = requiredHeight * ratio;
            }

            return size;
        };

        $scope.onRoomClick = function(room) {
            $scope.$emit('roomClicked', room);
        };

        $scope.drawRooms = function() {
            roomManageService.setSavedRoomCount($scope.floor.rooms.length);
            for (var i = 0; i < $scope.floor.rooms.length; i++) {
                $scope.drawRoom($scope.floor.rooms[i]);
                zoom.addZoomablePath($scope.floor.rooms[i].drawing);
            }
        };

        $scope.removeRoom = function(room) {
            if (room.drawing) {
                room.drawing.remove();
            }

            if (room.mark) {
                room.mark.remove();
            }
        };

        $scope.clearRooms = function() {
            for (var i = 0; i < $scope.floor.rooms.length; i++) {
                $scope.removeRoom($scope.floor.rooms[i]);
            }
        };

        $scope.redrawRooms = function(add, remove) {
            if (remove.length) {
                angular.forEach(remove, function(value, key) {
                    value.drawing.remove();
                });
            }

            if (add.length) {
                angular.forEach(add, function(value, key) {
                    $scope.drawRoom(value);
                });
            }
        };

        $scope.selectRoom = function(room) {
            self.roomClickHandler(room);
            $scope.selectedRoom = room;
            $scope.internalSelect = true;

            room.drawing.attr({
                'stroke': '#000000',
                'fill-opacity': '0.4',
                'stroke-width': 2
            });
        };

        $scope.deselectRoom = function(room) {
            if (!room) {
                room = $scope.selectedRoom;
            }

            if (!room) {
                return;
            }

            room.selected = false;
            $scope.selectedRoom = null;
            room.drawing.attr({
                'opacity': self.opacity,
                'stroke-width': 0
            });
        };

        $scope.selectRoomById = function(id) {
            var selectedRoom;

            id = parseInt(id);

            angular.forEach($scope.floor.rooms, function(room) {
                if (room.id === id) {
                    $scope.selectRoom(room);
                    selectedRoom = room;
                    return;
                }
            });

            return selectedRoom;
        };

        var timeout;
        var zoomFunc = function(zoomIn) {
            if (timeout) {
                zoom.setOptions({
                    enableZoom: true,
                    enablePan: true
                });
                zoom.zoom(zoomIn ? -0.1 : 0.1);
            }
        };

        $scope.zoomIn = function() {
            timeout = $timeout(function () { zoomFunc(false); }, 100);
        };

        $scope.cancelZoom = function() {
            $timeout.cancel(timeout);
        };

        $scope.zoomOut = function() {
            timeout = $timeout(function () { zoomFunc(true); }, 100);
        };

        $scope.clickZoomIn = function() {
            zoom.zoom(0.1);
        };

        $scope.clickZoomOut = function() {
            zoom.zoom(-0.1);
        };

        $scope.resetPan = function() {
            zoom.reset(300);
        };

        this.restoreClickOpacity = function restoreClickOpacity() {
            for (var i = 0; i < $scope.floor.rooms.length; i++) {
                $scope.floor.rooms[i].drawing.attr({
                    'opacity': this.opacity,
                    'stroke-width': 0
                });
                $scope.floor.rooms[i].selected = false;
            }
        };

        this.roomClickHandler = function(room) {
            if (handDrawingEnabled) {
                return;
            }

            $scope.selectedRoom = room;
            self.restoreClickOpacity();
            room.drawing.attr({
                opacity: 1
            });
            room.selected = true;
        };

        $scope.getPolygonCenter = function(coords) {
            var center = {
                x: 0,
                y: 0
            };
            for (var i = 0; i < coords.length; i++) {
                if ((i + 1) % 2 === 0) {
                    center.y += (coords[i] * $scope.aspectRatio);
                } else {
                    center.x += (coords[i] * $scope.aspectRatio);
                }
            }

            center.x /= (coords.length / 2);
            center.y /= (coords.length / 2);
            return center;
        };

        $scope.parseCoordinates = function(coords) {
            coords = coords.split(',');
            for (var i = 0; i < coords.length; i++) {
                coords[i] = parseFloat(coords[i]);
            }

            return coords;
        };

        $rootScope.enableHandDrawing = function() {
            handDrawingEnabled = true;
        };

        $rootScope.disableHandDrawing = function() {
            handDrawingEnabled = false;
        };

        $rootScope.isHandDrawingEnabled = function() {
            return handDrawingEnabled;
        };

        $scope.onRedraw = function(model) {
            roomManageService.onRedraw(model);
        };

        $scope.loadApplicationUsers = function(search) {
            return userRepository.getForAutocomplete(search);
        };

        $scope.clearUserDrawing = function() {
            $scope.arrayOfTempObjects.remove();
        };

        $scope.fillObjectsArray = function(object) {
            $scope.arrayOfTempObjects.push(object);
        };

        $scope.drawFirstDot = function(coords) {
            var firstDot = $scope.paper.ellipse(coords.x, coords.y, 7, 7).toFront()
                .attr({
                    'cursor': 'pointer',
                    'z-index': 1010,
                    'fill': '#FF0000',
                    'stroke': '#FF0000'
                });
            $scope.fillObjectsArray(firstDot);
        };

        $scope.drawNDot = function(coords) {
            var dot = $scope.paper.ellipse(coords.x, coords.y, 3, 3)
                .attr({
                    'fill': '#FF0000',
                    'stroke': '#FF0000',
                    'z-index': 1005
                });
            $scope.fillObjectsArray(dot);
        };

        $scope.drawPermLine = function(pathArray) {
            var path = 'M' + pathArray[0][1] + ' ' + pathArray[0][2] + ' L' + pathArray[1][1] + ' ' + pathArray[1][2];

            $scope.fillObjectsArray($scope.paper.path(path)
                .attr({
                    'stroke': '#FF0000',
                    'stroke-width': '2',
                    'opacity': 1
                }));
        };

        $scope.removeSpecificRoomDrawing = function(room) {
            $scope.removeRoom(room);
        };
    }
})();
