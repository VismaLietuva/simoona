var args = require('yargs').argv;
var config = require('./gulp.config')();
var tap = require('gulp-tap');
var del = require('del');
var gulp = require('gulp');
var path = require('path');
var file = require('gulp-file');
var ngConfig = require('gulp-ng-config');
var jeditor = require('gulp-json-editor');
var filter = require('gulp-filter');
var log = require('fancy-log');
var merge = require('gulp-merge-json');
var mergeStream = require('merge-stream');
var plumber = require('gulp-plumber');
var useref = require('gulp-useref');
var terser = require('gulp-terser');
var stripCssComments = require('gulp-strip-css-comments');
var _ = require('lodash');
var jsonminify = require('gulp-jsonminify');
var $ = require('gulp-load-plugins')({
    lazy: true
});

var port = process.env.PORT || config.defaultPort;

var buildDestination = config.build;
if (!args.deploy) {
    var browserSync = require('browser-sync');
}

/**
 * yargs variables can be passed in to alter the behavior, when present.
 * Example: gulp serve-dev
 *
 * --verbose  : Various tasks will produce more output to the console.
 * --nosync   : Don't launch the browser with browser-sync when serving code.
 * --debug    : Launch debugger with node-inspector.
 * --debug-brk: Launch debugger and break on 1st line with node-inspector.
 * --startServers: Will start servers for midway tests on the test task.
 */

/**
 * List the available gulp tasks
 */
gulp.task('help', $.taskListing);
gulp.task('default', gulp.series('help'));

/**
 * vet the code and create coverage report
 * @return {Stream}
 */
gulp.task('vet', function() {
    log('Analyzing source with JSHint and JSCS');

    return gulp
        .src(config.alljs)
        .pipe($.if(args.verbose, $.print()))
        .pipe($.jshint())
        .pipe($.jshint.reporter('jshint-stylish', {
            verbose: true
        }))
        .pipe($.jshint.reporter('fail'))
        .pipe($.jscs())
        .on('end', done);
});

/**
 * Remove all styles from the build and temp folders
 * @param  {Function} done - callback when complete
 */
gulp.task('clean-styles', function(done) {
    var files = [].concat(
        config.temp + '**/*.css',
        buildDestination + 'styles/**/*.css'
    );

    clean(files);
    done();
});

/**
 * Compile less to css
 * @return {Stream}
 */
gulp.task('styles-bootstrap', function(done) {
    log('Compiling Bootstrap Less --> CSS');

    return gulp
        .src(config.bootstrapLess)
        .pipe(plumber()) // exit gracefully if something fails after this
        .pipe($.less())
        .pipe($.autoprefixer())
        .pipe(gulp.dest(config.temp))
        .on('end', done);
});

/**
 * Compile less to css
 * @return {Stream}
 */
gulp.task('styles', gulp.series('clean-styles', 'styles-bootstrap', function(done) {
    log('Compiling Less --> CSS');

    return gulp
        .src(config.less)
        .pipe(plumber()) // exit gracefully if something fails after this
        .pipe($.less())
        .pipe($.autoprefixer())
        .pipe(gulp.dest(config.temp))
        .on('end', done);
}));

/**
 * Remove all fonts from the build folder
 * @param  {Function} done - callback when complete
 */
gulp.task('clean-fonts', function(done) {
    clean(buildDestination + 'fonts/**/*.*');
    done();
});

/**
 * Remove all translations from the build and temp folders
 * @param  {Function} done - callback when complete
 */
gulp.task('clean-translations', function(done) {
    var files = [].concat(
        config.client + 'resources/*.json',
        buildDestination + 'resources/*.json'
    );

    clean(files);
    done();
});

/**
 * Copy fonts
 * @return {Stream}
 */
gulp.task('fonts', gulp.series('clean-fonts', function(done) {
    log('Copying fonts');

    return gulp
        .src(config.fonts)
        .pipe(gulp.dest(buildDestination + 'fonts'))
        .on('end', done);
}));

/**
 * Merge translatoins
 * @return {Stream}
 */
gulp.task('translations', gulp.series('clean-translations', function(done) {
    log('Merging translation files');

    var stream = mergeStream();

    for (var i = 0; config.locales.length > i; i++) {
        var mergeJsonOptions = {
            fileName: 'locale-' + config.locales[i] + '.json'
        };

        var localeStream =
            gulp.src(config.client + 'resources/' + config.locales[i] + '/*.json')
            .pipe(plumber())
            .pipe(tap(function(file) {
                var fileName = path.basename(file.path).replace('.json', '');
                var json = JSON.parse(file.contents.toString());

                for (var prop in json) {
                    if (json.hasOwnProperty(prop)) {
                        json[fileName + '.' + prop] = json[prop];
                        delete json[prop];
                    }
                }

                file.contents = Buffer.from(JSON.stringify(json));
            }))
            .pipe(merge(mergeJsonOptions))
            .pipe(gulp.dest(config.client + 'resources'))
            .pipe(jsonminify())
            .pipe(gulp.dest(buildDestination + 'resources'));

        stream.add(localeStream);
    }

    return stream;
}));

/**
 * Copy extras
 * @return {Stream}
 */
gulp.task('extras', function() {
    log('Copying extra files');

    return gulp
        .src(config.extras, {
            dot: true
        })
        .pipe(gulp.dest(buildDestination));
});

/**
 * Remove all images from the build folder
 * @param  {Function} done - callback when complete
 */
gulp.task('clean-images', function(done) {
    clean(buildDestination + 'images/**/*.*');
    done();
});

/**
 * Remove all js and html from the build and temp folders
 * @param  {Function} done - callback when complete
 */
gulp.task('clean-code', function(done) {
    var files = [].concat(
        config.temp + '**/*.js',
        buildDestination + 'js/**/*.js',
        buildDestination + '**/*.html'
    );
    clean(files);
    done();
});

/**
 * Compress images
 * @return {Stream}
 */
gulp.task('images', gulp.series('clean-images', function(done) {
    log('Compressing and copying images');

    return gulp
        .src(config.images)
        .pipe($.imagemin({
            optimizationLevel: 4
        }))
        .pipe(gulp.dest(buildDestination + 'images'))
        .on('end', done);
}));

/**
 * Non compress images
 * @return {Stream}
 */
gulp.task('images-dev', gulp.series('clean-images', function(done) {
    log('Copying images');

    return gulp
        .src(config.images)
        .pipe(gulp.dest(buildDestination + 'images'))
        .on('end', done);
}));

gulp.task('less-watcher', function(done) {
    gulp.watch([config.less], gulp.series('styles'));
    done();
});

/**
 * Create $templateCache from the html templates
 * @return {Stream}
 */
gulp.task('templatecache', gulp.series('clean-code', function(done) {
    log('Creating an AngularJS $templateCache');

    return gulp
        .src(config.htmltemplates)
        .pipe($.if(args.verbose, $.bytediff.start()))
        .pipe($.minifyHtml({
            empty: true,
            loose: true
        }))
        .pipe($.if(args.verbose, $.bytediff.stop(bytediffFormatter)))
        .pipe($.angularTemplatecache(
            config.templateCache.file,
            config.templateCache.options
        ))
        .pipe(gulp.dest(config.temp))
        .on('end', done);
}));

/**
 * Create build config
 * @return {Stream}
 */
gulp.task('build-config', function(done) {
    var buildConfigs = {
        'build': config.defaultBuildConfig,
        'build-prod': config.productionBuildConfig,
        'build-dev': config.defaultBuildConfig
    };

    var build = buildConfigs[args._[0]];

    if (!build) {
        build = config.defaultBuildConfig;
    }

    var endpoint = args['endpoint'] || build.endpoint;
    var impersonate = args['impersonate'] || build.impersonate;
    var showMissingTranslations = args['showMissingTranslations'] || build.showMissingTranslations;
    var environment = args['environment'] || build.environment;

    config.FeatureFlag.key = args['hideFeature'] || config.FeatureFlag.key;
    config.FeatureFlag.active = !!args['hideFeature'];
    config.FeatureFlag.name = args['hideFeature'] || config.FeatureFlag.name;

    var featureFlags = [config.FeatureFlag];

    log('Endpoint: ' + endpoint);
    log('Impersonate is ' + (impersonate ? 'enabled' : 'disabled'));
    log('Show missing translations files is ' + (showMissingTranslations ? 'enabled' : 'disabled'));
    log('Environment: ' + environment);

    return file('build-config.js', '{}', { src: true})
        .pipe(jeditor({
            'endPoint': endpoint,
            'impersonate': impersonate,
            'showMissingTranslations': showMissingTranslations,
            'featureFlagsConstant': featureFlags,
            'environment': environment
        }))
        .pipe(ngConfig('simoonaApp.Constant', {
            createModule: true
        }))
        .pipe(gulp.dest(config.clientApp))
        .on('end', done);
});

/**
 * Wire-up the bower dependencies
 * @return {Stream}
 */
gulp.task('wiredep', gulp.series('build-config', 'translations', function(done) {
    log('Wiring the bower dependencies into the html');

    var wiredep = require('wiredep').stream;
    var options = config.getWiredepDefaultOptions();

    // Only include stubs if flag is enabled = args.stubs ? [].concat(config.js, config.stubsjs) :
    var js = config.js;

    return gulp
        .src(config.index)
        .pipe(wiredep(options))
        .pipe(inject(js, '', config.jsOrder))
        .pipe(gulp.dest(config.client))
        .on('end', done);
}));

gulp.task('inject', gulp.series('styles', 'templatecache', 'wiredep', function(done) {
    log('Wire up css into the html, after files are ready');

    return gulp
        .src(config.index)
        .pipe(inject(config.cssBootstrap, 'cssBootstrap'))
        .pipe(inject(config.css, 'css'))
        .pipe(gulp.dest(config.client))
        .on('end', done);
}));

gulp.task('move-locales', function(done) {
    log('Moving AngularJS locales to resources');

    return gulp
        .src(config.angularLocales)
        .pipe(gulp.dest(buildDestination + 'resources'))
        .on('end', done);
});

/**
 * Optimize all files, move to a build folder,
 * and inject them into the new index.html
 * @return {Stream}
 */
gulp.task('optimize', gulp.series('move-locales', 'inject', function(done) { //'test'
    log('Optimizing the js, css, and html');

    var stripCssOptions = {
        preserve: false
    };

    // Filters are named for the gulp-useref path
    var cssFilter = filter('**/*.css', { restore: true });
    var jsAppFilter = filter('**/' + config.optimized.app, { restore: true });
    var jsToplibFilter = filter('**/' + config.optimized.toplib, { restore: true });
    var jsBotlibFilter = filter('**/' + config.optimized.botlib, { restore: true });
    var notIndexFilter = filter(['**/*', '!**/index.html'], { restore: true });

    var templateCache = config.temp + config.templateCache.file;

    return gulp
        .src(config.index)
        .pipe(plumber())
        .pipe(inject(templateCache, 'templates'))
        .pipe(useref({ searchPath: './' })) // Gather all assets from the html with useref
        // Get the css
        .pipe(cssFilter)
        .pipe($.csso())
        .pipe(stripCssComments(stripCssOptions))
        .pipe(cssFilter.restore)
        // Get the custom javascript
        .pipe(jsAppFilter)
        .pipe($.ngAnnotate({
             add: true
         }))
        .pipe(terser())
        .pipe(jsAppFilter.restore)
        // Get the vendor javascript
        .pipe(jsToplibFilter)
        .pipe(terser()) // another option is to override wiredep to use min files
        .pipe(jsToplibFilter.restore)
        .pipe(jsBotlibFilter)
        .pipe(terser()) // another option is to override wiredep to use min files
        .pipe(jsBotlibFilter.restore)
        // Take inventory of the file names for future rev numbers
        .pipe(notIndexFilter)
        .pipe($.rev())
        // Apply the concat and file replacement with useref
        .pipe(notIndexFilter.restore)
        // Replace the file names in the html with rev numbers
        .pipe($.revReplace())
        .pipe(gulp.dest(buildDestination))
        .on('end', done);
}));

/**
 * Optimize all files, move to a build folder,
 * and inject them into the new index.html
 * @return {Stream}
 */
gulp.task('optimize-dev', gulp.series('move-locales', 'inject', function(done) {
    log('Optimizing for development the js, css, and html');

    // Filters are named for the gulp-useref path
    var cssFilter = filter('**/*.css', { restore: true });
    var jsAppFilter = filter('**/' + config.optimized.app, { restore: true });
    var jsToplibFilter = filter('**/' + config.optimized.toplib, { restore: true });
    var jsBotlibFilter = filter('**/' + config.optimized.botlib, { restore: true });
    var notIndexFilter = filter(['**/*', '!**/index.html'], { restore: true });

    var templateCache = config.temp + config.templateCache.file;

    return gulp
        .src(config.index)
        .pipe(plumber())
        .pipe(inject(templateCache, 'templates'))
        .pipe(useref({ searchPath: './' })) // Gather all assets from the html with useref
        // Get the css
        .pipe(cssFilter)
        .pipe(cssFilter.restore)
        // Get the custom javascript
        .pipe(jsAppFilter)
        .pipe($.ngAnnotate({
            add: true
        }))
        .pipe(jsAppFilter.restore)
        // Get the vendor javascript
        .pipe(jsToplibFilter)
        .pipe(jsToplibFilter.restore)
        .pipe(jsBotlibFilter)
        .pipe(jsBotlibFilter.restore)
        // Take inventory of the file names for future rev numbers
        .pipe(notIndexFilter)
        .pipe($.rev())
        // Apply the concat and file replacement with useref
        .pipe(notIndexFilter.restore)
        // Replace the file names in the html with rev numbers
        .pipe($.revReplace())
        .pipe(gulp.dest(buildDestination))
        .on('end', done);
}));

/**
 * Build everything
 * This is separate so we can run tests on
 * optimize before handling image or fonts
 */
gulp.task('build', gulp.series('optimize', 'images', 'fonts', 'extras', function(done) {
    log('Building everything');

    var msg = {
        title: 'gulp build',
        subtitle: 'Deployed to the build folder',
        message: 'Finishing `gulp build`'
    };

    del(config.temp);
    log(msg);
    notify(msg);
    done();
}));

gulp.task('prod-extras', function(done) {
    log('Copying extra files');

    return gulp
        .src(config.prod_extras, {
            dot: true
        })
        .pipe(gulp.dest(buildDestination))
        .on('end', done);
});

gulp.task('build-prod', gulp.series('optimize', 'images', 'fonts', 'prod-extras', function(done) {
    log('Building everything for azure production');

    var msg = {
        title: 'gulp build-prod',
        subtitle: 'Deployed to the build folder',
        message: 'Finishing `gulp build-prod`'
    };

    del(config.temp);
    log(msg);
    notify(msg);

    done();
}));

/**
 * Build without optimizing for development
 * This is separate so we can run tests on
 * optimize before handling image or fonts
 */
gulp.task('build-dev', gulp.series('optimize-dev', 'images-dev', 'fonts', 'extras', function(done) {
    log('Building development build');

    var msg = {
        title: 'gulp build-dev',
        subtitle: 'Deployed to the build folder',
        message: 'Finishing `gulp build-dev`'
    };

    del(config.temp);
    log(msg);
    notify(msg);

    done();
}));

/**
 * Remove all files from the build, temp, and reports folders
 * @param  {Function} done - callback when complete
 */
gulp.task('clean', function(done) {
    var delconfig = [].concat(buildDestination, config.temp, config.report);
    log('Cleaning: ' + delconfig);
    del(delconfig, { force: true });

    done();
});

/**
 * Run specs once and exit
 * To start servers and run midway specs as well:
 *    gulp test --startServers
 * @return {Stream}
 */
gulp.task('test', function(done) {
    startTests(true /*singleRun*/ , done);
});

/**
 * Run specs and wait.
 * Watch for file changes and re-run tests on each change
 * To start servers and run midway specs as well:
 *    gulp autotest --startServers
 */
gulp.task('autotest', function(done) {
    startTests(false /*singleRun*/ , done);
});

/**
 * serve the dev environment
 * --debug-brk or --debug
 * --nosync
 */
gulp.task('serve-dev', gulp.series('inject', function(done) {
    serve(true /*isDev*/);
    done();
}));

/**
 * serve the build environment
 * --debug-brk or --debug
 * --nosync
 */
gulp.task('serve-build', gulp.series('build', function(done) {
    serve(false /*isDev*/);
    done();
}));

/**
 * Optimize the code and re-load browserSync
 */
if (!args.deploy) {
    gulp.task('bsReload', gulp.series('optimize'), browserSync.reload);
    gulp.task('bsBuildStylesAndReload', gulp.series('styles'), browserSync.reload);
}

/**
 * When files change, log it
 * @param  {String} path - The path of the file that changed. If the cwd option was set, the path will be made relative by removing the cwd.
 * @param  {Object} stats - An fs.Stat object, but could be undefined. If the alwaysStat option was set to true, stats will always be provided.
 */
function changeEvent(path, stats) {
    var srcPattern = new RegExp('/.*(?=/' + config.source + ')/');
    log('File ' + path.replace(srcPattern, ''));
}

/**
 * Delete all files in a given path
 * @param  {Array}   path - array of paths to delete
 * @param  {Function} done - callback when complete
 */
function clean(path) {
    log('Cleaning: ' + path);
    del(path, {
        force: true
    });
}

/**
 * Inject files in a sorted sequence at a specified inject label
 * @param   {Array} src   glob pattern for source files
 * @param   {String} label   The label name
 * @param   {Array} order   glob pattern for sort order of the files
 * @returns {Stream}   The stream
 */
function inject(src, label, order) {
    var options = {};
    if (label) {
        options.name = 'inject:' + label;
    }

    return $.inject(orderSrc(src, order), options);
}

/**
 * Order a stream
 * @param   {Stream} src   The gulp.src stream
 * @param   {Array} order Glob array pattern
 * @returns {Stream} The ordered stream
 */
function orderSrc(src, order) {
    //order = order || ['**/*'];
    return gulp
        .src(src)
        .pipe($.if(order, $.order(order)));
}

/**
 * serve the code
 * --debug-brk or --debug
 * --nosync
 * @param  {Boolean} isDev - dev or build mode
 * @param  {Boolean} specRunner - server spec runner html
 */
function serve(isDev, specRunner) {
    var debug = args.debug || args.debugBrk;
    var debugMode = args.debug ? '--debug' : args.debugBrk ? '--debug-brk' : '';
    var nodeOptions = getNodeOptions(isDev);

    if (debug) {
        runNodeInspector();
        nodeOptions.nodeArgs = [debugMode + '=5858'];
    }

    if (args.verbose) {
        console.log(nodeOptions);
    }

    return $.nodemon(nodeOptions)
        .on('restart', [], function(ev) { //'vet'
            log('*** nodemon restarted');
            log('files changed:\n' + ev);
            setTimeout(function() {
                browserSync.notify('reloading now ...');
                browserSync.reload({
                    stream: false
                });
            }, config.browserReloadDelay);
        })
        .on('start', function() {
            log('*** nodemon started');
            startBrowserSync(isDev, specRunner);
        })
        .on('crash', function() {
            log('*** nodemon crashed: script crashed for some reason');
        })
        .on('exit', function() {
            log('*** nodemon exited cleanly');
        });
}

function getNodeOptions(isDev) {
    return {
        script: config.nodeServer,
        delayTime: 1,
        env: {
            'PORT': port,
            'NODE_ENV': isDev ? 'dev' : 'build'
        },
        watch: [config.server]
    };
}

function runNodeInspector() {
    log('Running node-inspector.');
    log('Browse to http://localhost:8080/debug?port=5858');
    var exec = require('child_process').exec;
    exec('node-inspector');
}

/**
 * Start BrowserSync
 * --nosync will avoid browserSync
 */
function startBrowserSync(isDev, specRunner) {
    if (args.nosync || browserSync.active) {
        return;
    }

    log('Starting BrowserSync on port ' + port);

    // If build: watches the files, builds, and restarts browser-sync.
    // If dev: watches less, compiles it to css, browser-sync handles reload
    if (isDev) {
        gulp.watch([config.allLess], gulp.series('bsBuildStylesAndReload'))
            .on('change', changeEvent);

        gulp.watch([config.client + '**/*.*', '!' + config.client + '**/*.less'])
            .on('change', function(path, stats) {
                changeEvent(path, stats); browserSync.reload();
            });

        gulp.watch([config.resources], gulp.series('translations'))
            .on('change', changeEvent);
    } else {
        gulp.watch([config.allLess, config.js, config.htmltemplates], gulp.series('bsReload'))
            .on('change', changeEvent);
    }

    log('Browser sync: ' + port);
    var options = {
        proxy: 'http://localhost:' + port,
        port: 3000,
        ghostMode: { // these are the defaults t,f,t,t
            clicks: true,
            location: false,
            forms: true,
            scroll: true
        },
        injectChanges: true,
        logFileChanges: true,
        logLevel: 'debug',
        logPrefix: 'gulp-patterns',
        notify: true,
        //https: true,
        reloadDelay: 0 //1000
    };
    if (specRunner) {
        options.startPath = config.specRunnerFile;
    }

    browserSync(options);
}

/**
 * Start the tests using karma.
 * @param  {boolean} singleRun - True means run once and end (CI), or keep running (dev)
 * @param  {Function} done - Callback to fire when karma is done
 * @return {undefined}
 */
function startTests(singleRun, done) {
    var excludeFiles = [];
    var karma = require('karma').Server;

    karma.start({
        configFile: __dirname + '/karma.conf.js',
        exclude: excludeFiles,
        singleRun: !!singleRun
    }, function(karmaResult) {
        log('Karma completed');
        if (karmaResult === 1) {
            done('karma: tests failed with code ' + karmaResult);
        } else {
            done();
        }
    });
}

/**
 * Formatter for bytediff to display the size changes after processing
 * @param  {Object} data - byte data
 * @return {String}      Difference in bytes, formatted
 */
function bytediffFormatter(data) {
    var difference = (data.savings > 0) ? ' smaller.' : ' larger.';
    return data.fileName + ' went from ' +
        (data.startSize / 1000).toFixed(2) + ' kB to ' +
        (data.endSize / 1000).toFixed(2) + ' kB and is ' +
        formatPercent(1 - data.percent, 2) + '%' + difference;
}

/**
 * Log an error message and emit the end of a task
 */
function errorLogger(error) {
    log('*** Start of Error ***');
    log(error);
    log('*** End of Error ***');
    this.emit('end');
}

/**
 * Format a number as a percentage
 * @param  {Number} num       Number to format as a percent
 * @param  {Number} precision Precision of the decimal
 * @return {String}           Formatted perentage
 */
function formatPercent(num, precision) {
    return (num * 100).toFixed(precision);
}

/**
 * Format and return the header for files
 * @return {String}           Formatted file header
 */
function getHeader() {
    var pkg = require('./package.json');
    var template = ['/**',
        ' * <%= pkg.name %> - <%= pkg.description %>',
        ' * @authors <%= pkg.authors %>',
        ' * @version v<%= pkg.version %>',
        ' * @link <%= pkg.homepage %>',
        ' * @license <%= pkg.license %>',
        ' */',
        ''
    ].join('\n');
    return $.header(template, {
        pkg: pkg
    });
}

/**
 * Log a message or series of messages using chalk's blue color.
 * Can pass in a string, object or array.
 */
function log(msg) {
    if (typeof(msg) === 'object') {
        for (var item in msg) {
            if (msg.hasOwnProperty(item)) {
                log(msg[item]);
            }
        }
    } else {
        log(msg);
    }
}

/**
 * Show OS level notification using node-notifier
 */
function notify(options) {
    var notifier = require('node-notifier');
    var notifyOptions = {
        sound: 'Bottle',
        contentImage: path.join(__dirname, 'gulp.png'),
        icon: path.join(__dirname, 'gulp.png')
    };
    _.assign(notifyOptions, options);
    notifier.notify(notifyOptions);
}

module.exports = gulp;
