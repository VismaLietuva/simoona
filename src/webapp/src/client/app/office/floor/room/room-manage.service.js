'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Office.Floor.Room');

    simoonaApp.factory('roomManageService', [
        '$rootScope', 'roomTypeRepository', '$uibModal', '$timeout', function($rootScope, roomTypeRepository, $uibModal, $timeout) {
            var coordsPath = '';
            var mapScope;
            var roomScope;
            var ranges = {};
            var lastPoint = {};
            var lastPath = '';
            var isAlowed = true;
            var roomsCount;
            var originalRoom = {};
            var isRedrawMode = false;
            var roomPopup;

            // Constants
            var firstEllipseRange = 7;

            // Function opens popup in dinamic location
            function openPopup() {
                roomPopup = $uibModal.open({
                    templateUrl: 'app/office/floor/room/room-manage-popup.html',
                    controller: 'roomManageController',
                    resolve: {
                        model: function() { return mapScope.model; },
                        floorid: function() { return mapScope.floor.id; },
                        editStage: function () { return roomScope.editStage; }
                    }
                });

                roomPopup.result.then(function() {
                }, function() {
                    // Popup was dissmised by user with ESC button or outside click.
                    roomManageService.cancel();
                });
            }

            //Function that saves coords to a string.
            //That strings purpose is to have hole path of the room.
            //It will be saved to DB and it will be used to draw a room
            //xxx,yyy,xxx,yyy.......
            function saveCoords(coords) {
                if (coordsPath !== '') {
                    coordsPath += ',';
                }

                //minus 32 needed because of the margins
                var x = parseInt(coords.x);
                //x -= 32;

                var y = parseInt(coords.y);
                //y -= 32;

                //divided because because when you draw by hand you are not effected by aspectRatio
                x /= mapScope.aspectRatio;
                y /= mapScope.aspectRatio;
                coordsPath += x.toString() + ',' + y.toString();
            }

            //Sets the first dot ranges for a click
            function setFirstEllipseRanges(coords) {
                ranges.xMax = coords.x + firstEllipseRange;
                ranges.xMin = coords.x - firstEllipseRange;
                ranges.yMax = coords.y + firstEllipseRange;
                ranges.yMin = coords.y - firstEllipseRange;
            }

            //Function checks already drawn paths with temp path.
            //Checks if those paths has intersections.
            //if true, so error message is shown and it sets isAlowed to false.
            //isAlowed controls drawing state.
            //If it is set to false, so you won't be able to draw a dot or a path
            function checkIntersections(path) {
                for (var i = 0; i < mapScope.arrayOfPaths.length; i++) {
                    var a = Raphael.pathIntersection(mapScope.arrayOfPaths[i], path);
                    if ((a !== undefined) && (a.length !== 0)) {
                        isAlowed = false;
                        showWarningMessage();
                        break;
                    } else {
                        isAlowed = true;
                        hideWarningMessage();
                    }
                }
            }


//Function that shows message "you cannot draw on other room"
            function showWarningMessage() {
                mapScope.warningMessage = true;
                mapScope.$apply();
            }

            //Function that hides message "you cannot draw on other room"
            function hideWarningMessage() {
                mapScope.warningMessage = false;
                mapScope.$apply();
            }

            //In this function is writen actions that is needed to take for first ellipse to appear
            function firstEllipseSet(coords) {
                setFirstEllipseRanges(coords);
                saveCoords(coords);
                mapScope.drawFirstDot(coords);
            }

            //Function that implement room drawing end.
            //In other words user has clicked on the first dot
            function firstEllipseClicked(coords) {
                mapScope.drawPermLine(lastPath.attrs.path);
                mapScope.fillObjectsArray(lastPath);
                $rootScope.disableHandDrawing();
                mapScope.clearUserDrawing();
                mapScope.model.coordinates = coordsPath;
                mapScope.drawRoom(mapScope.model);
                openPopup();
            }

            //Function implements actions that is needed to take to draw simple ellipse
            function NEllipseSet(coords) {
                saveCoords(coords);
                mapScope.drawNDot(coords);
                mapScope.drawPermLine(lastPath.attrs.path);
            }

            //Function checks if user has clicked on first ellipse
            //If not so it draws simple ellipse, representing room corner
            function firstEllipseClickCheck(coords) {
                if ((ranges.xMax > coords.x) && (ranges.xMin < coords.x) && (ranges.yMax > coords.y) && (ranges.yMin < coords.y)) {
                    firstEllipseClicked(coords);
                } else {
                    NEllipseSet(coords);
                }
            }

            //This function checks if user draw first or N ellipse(room corners)
            function checkEllipseState(coords) {
                if (ranges.xMax !== undefined) {
                    firstEllipseClickCheck(coords);
                } else {
                    firstEllipseSet(coords);
                }
            }

            function variableDeletionDependingOnMode(mode) {
                //Variables which is needed to clear always, not depending which button is clicked
                ranges = {};
                lastPoint = {};
                lastPath = '';
                isAlowed = true;
                coordsPath = '';
                mapScope.editStage = roomScope.editStage;

                if (roomsCount !== mapScope.arrayOfPaths.length) {
                    mapScope.removeRoom(mapScope.model);
                    mapScope.arrayOfPaths.pop();
                    mapScope.arrayOfRooms.pop();
                }
                mapScope.clearUserDrawing();

                if (mode === 'Create') {
                    originalRoom = {};
                    mapScope.model = {};
                }
            }


            function redrawProcedure() {
                $rootScope.enableHandDrawing();
                variableDeletionDependingOnMode('Redraw');
                mapScope.infoMessage = true;
            }

            function saveOriginalRoom(model) {
                angular.extend(originalRoom, model);
            }

            //Just these functions need to have contact with outer world
            var roomManageService = {
                //Function clear variables when button is pressed
                //this function is not finished, because the model is erased or not depending on what button is clicked
                clearVariables: function(mode) {
                    variableDeletionDependingOnMode(mode);
                },

                //This function draw one line and the just changes coordinates from where is starting
                drawTempLine: function (coords) {
                    if ($rootScope.isHandDrawingEnabled() && (lastPoint.x !== undefined)) {
                        var path = "M" + lastPoint.x + " " + lastPoint.y + "L" + coords.x + " " + coords.y;
                        checkIntersections(path);
                        if (isAlowed) {
                            if (lastPath === '') {
                                lastPath = mapScope.paper.path(path);
                            } else {
                                lastPath.attr("path", path);
                            }
                            lastPath.attr("stroke", "#FF0000");
                            lastPath.attr("stroke-width", "2");
                            lastPath.attr("opacity", 1);
                        }
                    }
                },

                //This function draw one line and the just changes coordinates from where is starting
                clearTempLine: function() {
                    var path = "M0 0 L0 0";

                    if (lastPath === '') {
                        lastPath = mapScope.paper.path(path);
                    } else {
                        lastPath.attr("path", path);
                    }
                    lastPath.attr("stroke", "#FF0000");
                    lastPath.attr("stroke-width", "2");
                    lastPath.attr("opacity", 1);
                },

                //function that checks if hand drawing is enabled and if is alowed to draw dot
                checkPoints: function(coords) {
                    if ($rootScope.isHandDrawingEnabled() && isAlowed) {
                        coords.y = parseInt(coords.y);
                        checkEllipseState(coords);
                        lastPoint.x = coords.x;
                        lastPoint.y = coords.y;
                        return true;
                    } else {
                        return false;
                    }
                },

                //Function that shows message "please click on a map to start drawing"
                showInfoMessage: function() {
                    mapScope.infoMessage = true;
                },

                //Function that hides message "please click on a map to start drawing"
                hideInfoMessage: function() {
                    mapScope.infoMessage = false;
                },

                //Function that sets the scope of a map
                setMapScope: function(scope) {
                    mapScope = scope;
                },

                //Function sets room scope
                setRoomScope: function(scope) {
                    roomScope = scope;
                },

                //This function is purposed that outer scope could reach popup and open if needed
                openPopupFromOutside: function(model) {
                    mapScope.model = model;
                    openPopup(location);
                },

                setSavedRoomCount: function(count) {
                    roomsCount = count;
                },

                getOriginalRoom: function() {
                    return originalRoom;
                },

                onRedraw: function(model) {
                    var foundedRoom = {};
                    for (var i = 0; i < mapScope.floor.rooms.length; i++) {
                        if (model.id === mapScope.floor.rooms[i].id) {
                            foundedRoom = mapScope.floor.rooms[i];
                            break;
                        }
                    }
                    if (roomScope.editStage === true) {
                        isRedrawMode = true;
                        saveOriginalRoom(model);
                        mapScope.removeSpecificRoomDrawing(foundedRoom);
                        var index = mapScope.arrayOfRooms.indexOf(foundedRoom);
                        if (index !== -1) {
                            mapScope.arrayOfPaths.splice(index, 1);
                            mapScope.arrayOfRooms.splice(index, 1);
                            roomsCount -= 1;
                        }
                    }
                    variableDeletionDependingOnMode();
                    mapScope.model = model;
                    redrawProcedure();
                    isRedrawMode = true;
                },

                checkRedrawMode: function() {
                    if ((isRedrawMode) && (originalRoom.id !== undefined)) {
                        var foundedRoom = {};
                        for (var i = 0; i < mapScope.floor.rooms.length; i++) {
                            if (originalRoom.id === mapScope.floor.rooms[i].id) {
                                foundedRoom = mapScope.floor.rooms[i];
                                break;
                            }
                        }
                        mapScope.isRedrawMode = true;
                        mapScope.drawRoom(foundedRoom);
                        isRedrawMode = false;
                    }
                },

                enableRedrawMode: function() {
                    mapScope.redrawMode = true;
                },

                disableRedrawMode: function() {
                    mapScope.redrawMode = false;
                },

                reloadPage: function() {
                    roomScope.getRooms();
                },

                deleteOriginalDrawing: function() {
                    mapScope.removeSpecificRoomDrawing(originalRoom);
                    originalRoom = {};
                },

                cancel: function() {
                    $rootScope.disableHandDrawing();
                    roomManageService.clearTempLine();
                    if (mapScope.model.drawing)
                        mapScope.deselectRoom(mapScope.model);
                    roomManageService.checkRedrawMode();
                    roomManageService.disableRedrawMode();
                    roomManageService.clearVariables('Create');
                    this.disableProgress();

                    $timeout(function() {
                        roomScope.$apply();
                    }, 0);
                },

                enableProgress: function() {
                    roomScope.isInProgress = true;
                },

                disableProgress: function() {
                    roomScope.isInProgress = false;
                },

                progressState: function() {
                    return roomScope.isInProgress;
                }
            }

            return roomManageService;
        }
    ]);
})();
