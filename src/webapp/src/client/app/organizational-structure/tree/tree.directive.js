(function () {
    'use strict';

    angular
        .module('simoonaApp.OrganizationalStructure')
        .directive('aceOrganizationTree', organizationTree);

    organizationTree.$inject = [
        '$window',
        'organizationalStructureRepository'
    ];

    function organizationTree($window, organizationalStructureRepository) {
        var directive = {
            restrict: 'AE',
            replace: true,
            scope: {
                resetTree: '&'
            },
            link: linkFunc
        };
        return directive;

        ///////

        function linkFunc(scope, el) {

            init();

            ////////////

            function init() {
                organizationalStructureRepository.getStructure().then(function (response) {
                    initTree(response);
                });
            }

            function initTree(orgData) {
                var nodes;
                var domNode;

                // Calculate total nodes, max label length
                var totalNodes = 0;
                var maxLabelLength = 0;

                // Misc. variables
                var i = 0;
                var duration = 750;
                var root;
                var svgGroup;
                var windowSizeOffset = 200;

                // size of the diagram
                var minWidthHeight = 750;
                var w = window,
                    doc = document,
                    elem = doc.documentElement,
                    body = doc.getElementsByTagName('body')[0],
                    x = (w.innerWidth || elem.clientWidth || body.clientWidth) - windowSizeOffset,
                    y = (w.innerHeight || elem.clientHeight || body.clientHeight) - windowSizeOffset;

                var viewerWidth = x < minWidthHeight ? minWidthHeight : x;
                var viewerHeight = y < minWidthHeight ? minWidthHeight : y;

                var tree = d3.layout.tree()
                    .size([viewerHeight, viewerWidth]);

                scope.resetTreeFunc = function () {
                    expandAll();
                    centerTree();
                };

                scope.resetTree({
                    resetTreeFn: scope.resetTreeFunc
                });

                // define a d3 diagonal projection for use by the node paths later on.
                var diagonal = d3.svg.diagonal()
                    .projection(function (d) {
                        return [d.y, d.x];
                    });

                // A recursive helper function for performing some setup by walking through all nodes
                function visit(parent, visitFn, childrenFn) {
                    if (!parent) {
                        return;
                    }

                    visitFn(parent);

                    var children = childrenFn(parent);
                    if (children) {
                        var count = children.length;
                        for (var i = 0; i < count; i++) {
                            visit(children[i], visitFn, childrenFn);
                        }
                    }
                }

                $(document).one('click', function (event) {
                    event.preventDefault();
                    $(this).prop('disabled', true);
                });

                // Call visit function to establish maxLabelLength
                visit(orgData, function (d) {
                    totalNodes++;
                    maxLabelLength = Math.max(d.fullName.length, maxLabelLength);
                }, function (d) {
                    return d.children && d.children.length > 0 ? d.children : null;
                });

                // sort the tree according to the node names
                function sortTree() {
                    tree.sort(function (a, b) {
                        return b.fullName.toLowerCase() < a.fullName.toLowerCase() ? 1 : -1;
                    });
                }

                // Sort the tree initially incase the JSON isn't in a sorted order.
                sortTree();

                // define the zoomListener which calls the zoom function
                // on the 'zoom' event constrained within the scaleExtents
                var zoomListener = d3.behavior.zoom().scaleExtent([0.7, 1.8]).on('zoom', zoom);

                // Define the zoom function for the zoomable tree
                function zoom() {
                    svgGroup
                        .attr('transform', 'translate(' + d3.event.translate + ')scale(' + d3.event.scale + ')');
                }

                // define the baseSvg, attaching a class for styling and the zoomListener
                var baseSvg = d3.select(el[0]).append('svg')
                    .attr('width', viewerWidth)
                    .attr('height', viewerHeight)
                    .attr('class', 'overlay')
                    .call(zoomListener)
                    .on('dblclick.zoom', null);

                angular.element($window).bind('resize', function () {
                    x = (w.innerWidth || elem.clientWidth || body.clientWidth) - windowSizeOffset;
                    y = (w.innerHeight || elem.clientHeight || body.clientHeight) - windowSizeOffset;
                    viewerWidth = x < minWidthHeight ? minWidthHeight : x;
                    viewerHeight = y < minWidthHeight ? minWidthHeight : y;

                    baseSvg
                        .attr('width', viewerWidth)
                        .attr('height', viewerHeight);
                });

                // Helper functions for collapsing and expanding nodes.

                function collapse(d) {
                    if (d.children) {
                        d._children = d.children;
                        d._children.forEach(collapse);
                        d.children = null;
                    }
                }

                function expand(d) {
                    var children = (d.children) ? d.children : d._children;
                    if (d._children) {
                        d.children = d._children;
                        d._children = null;
                    }

                    if (children) {
                        children.forEach(expand);
                    }
                }

                function expandAll() {
                    expand(root);
                    update(root);
                }

                function centerTree(source) {
                    if (!source) {
                        source = root;
                    }

                    var scale = 1;
                    var x = -source.y0;
                    var y = -source.x0;
                    x = x * scale + (maxLabelLength * 12);
                    y = y * scale + viewerHeight / 3;
                    d3.select('g').transition()
                        .duration(duration)
                        .attr('transform', 'translate(' + x + ',' + y + ')scale(' + scale + ')');
                    zoomListener.scale(scale);
                    zoomListener.translate([x, y]);
                }

                function centerNode(source) {
                    var scale = zoomListener.scale();
                    var x = -source.y0;
                    var y = -source.x0;
                    x = x * scale + (maxLabelLength * 12);
                    y = y * scale + viewerHeight / 3;
                    d3.select('g').transition()
                        .duration(duration)
                        .attr('transform', 'translate(' + x + ',' + y + ')scale(' + scale + ')');
                    zoomListener.scale(scale);
                    zoomListener.translate([x, y]);
                }

                // Toggle children function
                function toggleChildren(d) {
                    if (d.children) {
                        d._children = d.children;
                        d.children = null;
                    } else if (d._children) {
                        d.children = d._children;
                        d._children = null;
                    }

                    return d;
                }

                // Toggle children on click.
                function click(d) {
                    if (d3.event.defaultPrevented) {
                        return; // click suppressed
                    }

                    if (d.children || d._children) {
                        d = toggleChildren(d);
                        update(d);
                        centerNode(d);
                    }
                }

                function clickText(d) {
                    if (d3.event.defaultPrevented) {
                        return; // click suppressed
                    }
                    // TODO: will be tooltip displaying on click
                }

                function update(source) {
                    // Compute the new height, function counts total children of root node and
                    // sets tree height accordingly.
                    // This prevents the layout looking squashed when new nodes are made visible
                    // or looking sparse when nodes are removed
                    // This makes the layout more consistent.
                    var levelWidth = [1];
                    var childCount = function (level, n) {
                        if (n.children && n.children.length > 0) {
                            if (levelWidth.length <= level + 1) {
                                levelWidth.push(0);
                            }

                            levelWidth[level + 1] += n.children.length;
                            n.children.forEach(function (d) {
                                childCount(level + 1, d);
                            });
                        }
                    };

                    childCount(0, root);
                    var newHeight = d3.max(levelWidth) * 30; // line height
                    tree = tree.size([newHeight, viewerWidth]);

                    // Compute the new tree layout.
                    var nodes = tree.nodes(root).reverse(),
                        links = tree.links(nodes);

                    // Set widths between levels based on maxLabelLength.
                    nodes.forEach(function (d) {
                        d.y = (d.depth * (maxLabelLength * 15));
                    });

                    // Update the nodes…
                    var node = svgGroup.selectAll('g.node')
                        .data(nodes, function (d) {
                            return d.id || (d.id = ++i);
                        });

                    // Enter any new nodes at the parent's previous position.
                    var nodeEnter = node.enter().append('g')
                        .attr('class', 'node')
                        .attr('transform', function (d) {
                            return 'translate(' + source.y0 + ',' + source.x0 + ')';
                        });

                    nodeEnter.append('circle')
                        .attr('class', 'nodeCircle')
                        .attr('r', 5.5)
                        .style('fill', function (d) {
                            return d._children ? 'lightsteelblue' : '#fff';
                        }).on('click', click);

                    nodeEnter.append('text')
                        .attr('dy', '.35em')
                        .attr('class', 'nodeText')
                        .text(function (d) {
                            return d.fullName;
                        })
                        .style('fill-opacity', 0);

                    // Change the circle fill depending on whether it has children and is collapsed
                    node.select('circle.nodeCircle')
                        .style('fill', function (d) {
                            return d._children ? 'lightsteelblue' : '#fff';
                        });

                    // Transition nodes to their new position.
                    var nodeUpdate = node
                        .transition()
                        .duration(duration)
                        .attr('transform', function (d) {
                            return 'translate(' + d.y + ',' + d.x + ')';
                        })
                        .call(endAll, function () {
                            redrawSvg();
                        });

                    // Fade the text in
                    nodeUpdate.select('text')
                        .style('fill-opacity', 1);

                    // Transition exiting nodes to the parent's new position.
                    var nodeExit = node.exit().transition()
                        .duration(duration)
                        .attr('transform', function (d) {
                            return 'translate(' + source.y + ',' + source.x + ')';
                        })
                        .remove();

                    nodeExit.select('text')
                        .style('fill-opacity', 0);

                    nodeEnter.selectAll('text.nodeText').attr('x', function (d) {
                        return d.children || d._children ? -this.getComputedTextLength() - 10 : 10;
                    }).text(function (d) {
                        return d.fullName;
                    });

                    // Update the links…
                    var link = svgGroup.selectAll('path.link')
                        .data(links, function (d) {
                            return d.target.id;
                        });

                    // Enter any new links at the parent's previous position.
                    link.enter().insert('path', 'g')
                        .attr('class', 'link')
                        .attr('d', function (d) {
                            var o = {
                                x: source.x0,
                                y: source.y0
                            };
                            return diagonal({
                                source: o,
                                target: o
                            });
                        });

                    // Transition links to their new position.
                    link.transition()
                        .duration(duration)
                        .attr('d', diagonal);

                    // Transition exiting nodes to the parent's new position.
                    link.exit().transition()
                        .duration(duration)
                        .attr('d', function (d) {
                            var o = {
                                x: source.x,
                                y: source.y
                            };
                            return diagonal({
                                source: o,
                                target: o
                            });
                        })
                        .remove();

                    // Stash the old positions for transition.
                    nodes.forEach(function (d) {
                        d.x0 = d.x;
                        d.y0 = d.y;
                    });
                }

                // Append a group which holds all nodes and which the zoom Listener can act upon.
                svgGroup = baseSvg.append('g');

                // Define the root
                root = orgData;
                root.x0 = viewerHeight / 2;
                root.y0 = 0;

                // Layout the tree initially and center on the root node.
                update(root);
                centerTree(root);
            }

            function redrawSvg() {
                var selector = document.getElementById('orgStructure');

                if (selector) {
                    selector.style.display = 'none';
                    selector.style.display = '';
                }
            }

            function endAll(transition, callback) {
                var size = 0;

                if (transition.empty()) {
                    callback();
                } else {
                    size = transition.size();
                    transition.each('end', function () {
                        size--;
                        if (size === 0) {
                            callback();
                        }
                    });
                }
            }
        }
    }
})();
