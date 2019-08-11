(function() {
    'use strict';

    angular
        .module('simoonaApp.Map')
        .directive('mapDir', mapDir);

    mapDir.$inject = ['$timeout', '$document', '$state', 'roomManageService', 'pinService', '$window', 'zoom', '$filter'];

    function mapDir($timeout, $document, $state, roomManageService, pinService, $window, zoom, $filter) {
        var directive = {
            templateUrl: 'app/office/map/map.html',
            restrict: 'E',
            scope: {
                allOffices: '=',
                office: '=',
                floor: '=',
                enableZoom: '=',
                enablePin: '=',
                params: '=',
                showFloorChange: '=',
                enableFullScreen: '=',
                mapControl: '=',
                heightOffset: '@'
            },
            transclude: true,
            replace: true,
            controller: 'mapController',
            link: function (scope, element, attrs) {
                if (!scope.enableFullScreen) {
                    $document.find('body').removeClass('map-body');
                }

                scope.container = element.find('#map-container');
                var width = element.width();
                scope.paper = new Raphael(scope.container[0]);
                scope.container.on('contextmenu', function(e) {
                    e.preventDefault();
                });

                scope.container.on('mouseup', function(e) {
                    if (zoom.panning) return;
                    if (!scope.clickedRoom || scope.clickedRoom.selected) {
                        scope.deselectRoom();
                        scope.$emit('roomUnselected');
                        scope.clickedRoom = null;
                        return;
                    }
                    scope.selectRoom(scope.clickedRoom);
                    scope.onRoomClick(scope.clickedRoom);
                    scope.clickedRoom = null;
                });

                scope.arrayOfTempObjects = scope.paper.set();
                scope.arrayOfPaths = [];
                scope.arrayOfRooms = [];

                scope.svg = element.find(scope.paper.canvas);
                scope.svg.hide();

                scope.zoomDisabled = !scope.enableZoom;

                var cWidth = scope.container.width();

                angular.element($window).on('resize', function(e) {
                    width = element.width();
                    handleFloorChange();
                    scope.$apply();
                    //scope.svg.panzoom('resetDimensions');
                });

                scope.$watch('params.highlightedRoomId',
                    function(newId, oldId) {
                        if (newId !== oldId && newId) {
                            var room = scope.selectRoomById(newId);
                            if (!room) return;
                            var coords = scope.parseCoordinates(room.coordinates);
                            var center = scope.getPolygonCenter(coords);
                            if (!zoom.isCoordsVisible(center.x, center.y)) {
                                var diff = zoom.getCoordsVisiblilityDiff(center.x, center.y);
                                zoom.pan(diff.x, diff.y, 300);
                                //zoom.panToCoordinates(center.x, center.y, 500);
                            }
                        }
                    });

                scope.internalControl = scope.mapControl || {};
                scope.internalControl.deselectRoom = function() {
                    scope.deselectRoom();
                };

                scope.$watch(function() {
                        return scope.floor;
                    },
                    function(newfloor) {
                        if (!newfloor) return;
                        if (newfloor.$promise && !newfloor.$resolved) {
                            newfloor.$promise.then(handleFloorChange);
                        } else {
                            handleFloorChange();
                        }
                    });

                function handleRoomChange(newCollection, oldCollection) {
                    scope.drawRooms();
                    scope.svg.fadeIn(500);
                }

                function handleFloorChange() {
                    if (scope.floor.picture) {
                        scope.svg.hide();
                        scope.paper.clear();
                        scope.zoomableDrawings = scope.paper.set();
                        changeBackground();
                        handleRoomChange();
                        drawRoom();
                    }
                }

                scope.officeImageOnLoad = function (width, height) {
                    scope.floor.picture = {
                        width: width,
                        height: height
                    };

                    handleFloorChange();
                };

                function changeBackground() {
                    var heightOffset = 0;
                    /*if (typeof (scope.heightOffset) !== 'undefined'){
                        heightOffset = scope.heightOffset;
                    }*/
                    zoom.init({
                        width: width,
                        // TODO: monitor map sizing/overflow behaviour - 68px looks extra
                        height: $document.height() - 134 - 51 - heightOffset, // - 68,
                        container: scope.svg,
                        paper: scope.paper
                    });

                    var picture = {
                        width: scope.floor.picture.width,
                        height: scope.floor.picture.height
                    };

                    var paperSize = {
                        width: scope.svg.width(),
                        height: scope.svg.height()
                    };

                    if (picture.width < paperSize.width && picture.height < paperSize.height) {
                        //scope.aspectRatio = 1;
                        scope.aspectRatio = paperSize.width / picture.width;
                    } else if (picture.width > picture.height) {
                        scope.aspectRatio = picture.width > paperSize.width ? paperSize.width / picture.width : picture.width / paperSize.width;
                        if (paperSize.height < picture.height * scope.aspectRatio) {
                            scope.aspectRatio = paperSize.height / picture.height;
                        }
                    } else {
                        scope.aspectRatio = picture.height > paperSize.height ? paperSize.height / picture.height : picture.height / paperSize.height;
                        if (paperSize.width < picture.width * scope.aspectRatio) {
                            scope.aspectRatio = paperSize.width / picture.width;
                        }
                    }

                    var backgroundSize = {
                        width: picture.width * scope.aspectRatio,
                        height: picture.height * scope.aspectRatio
                    };

                    scope.background = scope.paper
                        .image($filter('picture')(scope.floor.pictureId), 0, 0, backgroundSize.width, backgroundSize.height).attr({
                            'stroke': '#000000',
                            'stroke-width': 2
                        });

                    zoom.addZoomablePath(scope.background);

                    zoom.setOptions({
                        x: 0,//(zoom.width - backgroundSize.width) / 2,
                        maxZoom: 2,
                        aspectRatio: scope.aspectRatio,
                        enableZoom: scope.aspectRatio !== 1,
                        enablePan: scope.aspectRatio !== 1
                    });
                }

                function drawRoom() {
                    if ($state.current.name === 'Root.WithOrg.Admin.Offices.Floors.Rooms.List') {
                        scope.RoomState = true;
                        roomManageService.setMapScope(scope);
                        zoom.setOptions({
                            enableZoom: false,
                            enablePan: false
                        });
                        scope.svg.bind('mousedown', function(event) {
                            if (event.which === 1) {
                                var offset = scope.container.offset();

                                var coords = {
                                    x: event.pageX - parseInt(offset.left),
                                    y: event.pageY - parseInt(offset.top)
                                };

                                var state = roomManageService.checkPoints(coords, scope);
                                if (state) {
                                    roomManageService.hideInfoMessage();
                                    element.bind('mousemove', function(event) {
                                        var isFirefoxBrowser = window.navigator.userAgent.indexOf("Firefox") !== -1;
                                        var currentCoords = {
                                            x: event.offsetX === undefined || isFirefoxBrowser ? event.originalEvent.layerX : event.offsetX,
                                            y: event.offsetY === undefined || isFirefoxBrowser ? event.originalEvent.layerY : event.offsetY
                                        };
                                        roomManageService.drawTempLine(currentCoords);
                                    });
                                }
                            }
                        });
                    }
                }
            }
        };
        return directive;
    }
})();
