'use strict';

(function () {
    var simoonaApp = angular.module('simoonaApp.Map');

    simoonaApp.factory('zoom', [
        '$document', '$q', function($document, $q) {
            var currentZoom,
                increase = 0.1,
                mousedown,
                initialWidth,
                initialHeight,
                zoomDx = 0,
                zoomDy = 0,
                mouseout,
                startX,
                startY,
                matrix,
                panDx = 0,
                panDy = 0,
                setOptionsDeferrer = $q.defer(),
                setOptionsPromise = setOptionsDeferrer.promise;

            function updateDrawingDimensions(drawing) {
                if (!initialWidth && !initialHeight) {
                    var itemBBox = drawing.getBBox();
                    initialHeight = itemBBox.height;
                    initialWidth = itemBBox.width;
                }
            }

            var settings = {
                x: 0,
                y: 0,
                increase: 0.1,
                zoom: 1,
                enablePan: true,
                enableZoom: true,
                maxZoom: 5,
                minZoom: 0.5,
                aspectRatio: 1,
            };

            return {
                setOptionsPromise: function(){
                    return setOptionsPromise;
                },
                setOptions: function(options) {
                    angular.extend(settings, options);

                   

                    this.container = settings.container;
                    this.paper = settings.paper;
                    this.width = settings.width;
                    this.height = settings.height;
                    this.aspectRatio = settings.aspectRatio;

                    currentZoom = options.zoom ? options.zoom : currentZoom;
                    matrix = matrix && options.x !== undefined && options.y !== undefined
                        ? matrix : Raphael.matrix(1, 0, 0, 1, settings.x, settings.y);

                    if (options.x || options.y) {
                        this.zoomableDrawings.transform(matrix.toTransformString());
                        this.unzoomable.translate(options.x, options.y);
                    }

                    settings.minZoom = options.minZoom ? parseFloat(options.minZoom.toFixed(1)) : settings.minZoom;
                    settings.maxZoom = options.maxZoom ? parseFloat(options.maxZoom.toFixed(1)) : settings.maxZoom;

                    setOptionsDeferrer.resolve();
                },
                handleOptions: function(options) {
                    if (!options.container) throw '"container" is required as an option for zoom!';
                    if (!options.width) throw '"width" is required as an option for zoom!';
                    if (!options.height) throw '"height" is required as an option for zoom!';
                    if (!options.paper) throw '"paper" is required as an option for zoom!';
                    this.setOptions(options);
                },
                zoom: function(increase) {
                    if (!increase || !settings.enableZoom
                        || (settings.maxZoom && Math.toPrecise(currentZoom + increase) > settings.maxZoom)
                        || (settings.minZoom && Math.toPrecise(currentZoom + increase) < settings.minZoom)) {
                        return;
                    }

                    currentZoom = Math.toPrecise(currentZoom + increase);
                    matrix.a = currentZoom;
                    matrix.d = currentZoom;
                    var dx = Math.toPrecise(Math.toPrecise(initialWidth * -increase) / 2);
                    var dy = Math.toPrecise(Math.toPrecise(initialHeight * -increase) / 2);
                    zoomDx = Math.toPrecise(zoomDx + dx);
                    zoomDy = Math.toPrecise(zoomDy + dy);
                    matrix.e = Math.toPrecise(matrix.e + dx);
                    matrix.f = Math.toPrecise(matrix.f + dy);
                    this.zoomableDrawings.transform(matrix.toTransformString());

                    this.unzoomable.forEach(function(el) {
                        var elBBox = el.node.getBBox();
                        var posX = Math.toPrecise(Math.toPrecise(elBBox.x - Math.toPrecise(initialWidth / 2)) * increase) + elBBox.width / 2 * increase;
                        var posY = Math.toPrecise(Math.toPrecise(elBBox.y - Math.toPrecise(initialHeight / 2)) * increase) + elBBox.height / 2 * increase;
                        el.translate(posX, posY);
                    });
                },
                zoomInto: function(increase, x, y) {
                    if (!increase || !settings.enableZoom
                        || (settings.maxZoom && Math.toPrecise(currentZoom + increase) > settings.maxZoom)
                        || (settings.minZoom && Math.toPrecise(currentZoom + increase) < settings.minZoom)) {
                        return;
                    }

                    currentZoom = Math.toPrecise(currentZoom + increase);

                    var xRelativeToDrawings = Math.toPrecise(x - this.width / 2 + initialWidth / 2 - panDx);
                    // xRelatveToDrawings must be between [0; initialWidth]
                    if (xRelativeToDrawings < 0)
                        xRelativeToDrawings = 0;
                    if (xRelativeToDrawings > initialWidth)
                        xRelativeToDrawings = initialWidth;

                    var yRelativeToDrawings = y - panDy;
                    // yRelatveToDrawings must be between [0; initialHeight]
                    if (yRelativeToDrawings < 0)
                        yRelativeToDrawings = 0;
                    if (yRelativeToDrawings > initialHeight)
                        yRelativeToDrawings = initialHeight;

                    matrix.a = currentZoom;
                    matrix.d = currentZoom;
                    var dx = Math.toPrecise(xRelativeToDrawings * -increase);
                    var dy = Math.toPrecise(yRelativeToDrawings * -increase);

                    zoomDx = Math.toPrecise(zoomDx + dx);
                    zoomDy = Math.toPrecise(zoomDy + dy);
                    matrix.e = Math.toPrecise(matrix.e + dx);
                    matrix.f = Math.toPrecise(matrix.f + dy);

                    this.zoomableDrawings.transform(matrix.toTransformString());

                    this.unzoomable.forEach(function(el) {
                        var elBBox = el.node.getBBox();
                        var posX = Math.toPrecise(elBBox.x + elBBox.width / 2 - xRelativeToDrawings) * increase;
                        var posY = Math.toPrecise(elBBox.y + elBBox.height / 2 - yRelativeToDrawings) * increase;
                        el.translate(posX, posY);
                    });
                },
                pan: function(dx, dy, duration) {
                    if (!settings.enablePan) return;

                    if (matrix.e + dx <= -initialWidth * currentZoom + 200
                        || matrix.e + dx >= this.width - 200) {
                        dx = 0;
                    }

                    if (matrix.f + dy <= -initialHeight * currentZoom + 200
                        || matrix.f + dy >= this.height - 200) {
                        dy = 0;
                    }

                    matrix.e += dx;
                    matrix.f += dy;
                    panDx += dx;
                    panDy += dy;

                    this.zoomableDrawings.transform(matrix.toTransformString());
                    this.unzoomable.translate(dx, dy);
                },
                addZoomablePath: function(path) {
                    this.zoomableDrawings.push(path);
                    updateDrawingDimensions(this.zoomableDrawings);
                    this.zoomableDrawings.transform(matrix.toTransformString());
                },
                removeZoomablePath: function(path) {
                    this.zoomableDrawings.exclude(path);
                },
                addUnzoomablePath: function(path) {
                    path.transform('t' + settings.x + ',' + settings.y);
                    this.unzoomable.push(path);
                },
                removeUnzoomablePath: function(path) {
                    this.unzoomable.exclude(path);
                },
                init: function(options) {
                    this.handleOptions(angular.extend(settings, options));
                    this.paper.setSize(this.width, this.height);
                    this.zoomableDrawings = this.paper.set();
                    this.unzoomable = this.paper.set();
                    var self = this;
                    self.container.off('mousewheel');
                    self.container.off('mousedown');
                    self.container.off('mousemove');
                    self.container.off('mouseout');
                    self.container.off('click');
                    self.container.on('mousewheel', function(e) {
                            if (!settings.enableZoom) return;
                            e.preventDefault();
                            var delta = e.delta || e.originalEvent.wheelDelta;
                            var zoomOut = delta ? delta < 0 : e.originalEvent.deltaY > 0;
                            var sIncrease = zoomOut ? -increase : increase;
                            var coords = self.getCoordsRealativeToContainer(e.pageX, e.pageY);
                            self.zoomInto(sIncrease, coords.x, coords.y);
                        })
                        .on('mousedown', function(e) {
                            if (e.which !== 1 || !settings.enablePan) return;
                            mouseout = false;
                            mousedown = true;
                            startX = e.pageX;
                            startY = e.pageY;
                            self.panning = false;
                            e.preventDefault();
                        })
                        .on('mousemove', function(e) {
                            if (!settings.enablePan || !mousedown || e.pageX - startX === 0 || e.pageY - startY === 0) return;
                            angular.element(self).css({ cursor: 'move' });
                            self.pan(e.pageX - startX, e.pageY - startY);
                            startX = e.pageX;
                            startY = e.pageY;
                            self.panning = true;
                            e.preventDefault();
                        })
                        .on('mouseout', function(e) {
                            mouseout = true;
                            e.preventDefault();
                        })
                        .on('click', function(e) {
                            if (e.which !== 1) return;
                            angular.element(self).css({ cursor: 'inherit' });
                            mousedown = false;
                            self.panning = false;
                            e.preventDefault();
                        });

                    $document.on('mouseup', function(e) {
                        if (e.which !== 1) return;
                        if (mouseout) {
                            angular.element(self).css({ cursor: 'inherit' });
                            mousedown = false;
                            self.panning = false;
                        }
                        mouseout = false;
                        e.preventDefault();
                    });

                    setOptionsDeferrer = $q.defer(),
                    setOptionsPromise = setOptionsDeferrer.promise;
                },
                resetZoom: function(animationDelay) {
                    animationDelay = animationDelay ? animationDelay : 0;

                    matrix.e -= zoomDx;
                    matrix.f -= zoomDy;

                    currentZoom = 1;

                    matrix.a = currentZoom;
                    matrix.d = currentZoom;
                    this.zoomableDrawings.animate({
                        transform: matrix.toTransformString()
                    }, animationDelay);

                    this.unzoomable.forEach(function(el) {
                        var posX = settings.x + panDx;
                        var posY = settings.x + panDy;
                        el.animate({
                            transform: 't' + posX + ',' + posY
                        }, 100);
                    });

                    zoomDx = zoomDy = 0;
                },
                getCurrentZoom: function() {
                    return currentZoom;
                },

                resetPan: function(animationDelay) {
                    animationDelay = animationDelay ? animationDelay : 0;

                    matrix.e = settings.x + zoomDx;
                    matrix.f = settings.y + zoomDy;
                    this.zoomableDrawings.animate({
                        transform: matrix.toTransformString()
                    }, animationDelay);
                    this.unzoomable.animate({
                        transform: 't' + settings.x + ',' + settings.y
                    }, animationDelay);

                    panDx = panDy = 0;
                },
                reset: function(animationDelay) {
                    animationDelay = animationDelay ? animationDelay : 0;
                    this.resetZoom(animationDelay);
                    this.resetPan(animationDelay);
                },
                getCoordsRealativeToContainer: function(x, y) {
                    // Getting this.container (SVG) offset() is buggy in Firefox. Getting its' parent offset() should solve the issue.
                    var contOffset = this.container.parent().offset();
                    var coords = {
                        x: x - parseInt(contOffset.left),
                        y: y - parseInt(contOffset.top)
                    };
                    return coords;
                },
                getCoordsRelativeToDrawing: function(x, y) {
                    return {
                        x: x - settings.x,
                        y: y - settings.y
                    }
                },
                getCoordsRelativeToMatrix: function(x, y) {
                    // Getting this.container (SVG) offset() is buggy in Firefox. Getting its' parent offset() should solve the issue.
                    var contOffset = this.container.parent().offset();
                    return {
                        x: matrix.x(x, y) + parseInt(contOffset.left),
                        y: matrix.y(x, y) + parseInt(contOffset.top)
                    }
                },
                isCoordsVisible: function(x, y) {
                    x = matrix.x(x, y);
                    y = matrix.y(x, y);
                    return x < this.width && y < this.height && x > 0 && y > 0;
                },
                getCoordsVisiblilityDiff: function(x, y) {
                    // Getting this.container (SVG) offset() is buggy in Firefox. Getting its' parent offset() should solve the issue.
                    var contOffset = this.container.parent().offset();
                    x = matrix.x(x, y);
                    y = matrix.y(x, y);
                    var padding = 100;

                    if (x >= this.width) {
                        x = this.width - x - padding;
                    } else if (x <= 0) {
                        x = -x + padding;
                    } else {
                        x = 0;
                    }

                    if (y >= this.height) {
                        y = this.height - y - padding;
                    } else if (y <= 0) {
                        y = -y + padding;
                    } else {
                        y = 0;
                    }

                    return {
                        x: x,
                        y: y
                    };
                },
                //coordinates must be relative to drawing
                panToCoordinates: function(x, y, duration) {
                    x = this.width / 2 - (matrix.e + x * currentZoom);
                    y = this.height / 2 - (matrix.f + y * currentZoom);
                    this.pan(x, y, duration);
                },

                // TODO: getPolygonCenter() and parseCoords() methods came from map-ctrl.js. They are static methods, so were moved to a separate service. Consider removing them from map-ctrl.js and review their usage.
                getPolygonCenter: function(coords) {
                    var center = {
                        x: 0,
                        y: 0
                    };
                    for (var i = 0; i < coords.length; i++) {
                        if ((i + 1) % 2 == 0) {
                            center.y += (coords[i] * this.aspectRatio);
                        } else {
                            center.x += (coords[i] * this.aspectRatio);

                        }
                    }
                    center.x /= (coords.length / 2);
                    center.y /= (coords.length / 2);
                    return center;
                },

                parseCoords: function(coords) {
                    coords = coords.split(',');
                    for (var i = 0; i < coords.length; i++) {
                        coords[i] = parseFloat(coords[i]);
                    }
                    return coords;
                }
            }
        }
    ]);
})();