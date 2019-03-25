// Karma configuration
// Generated on Mon Jan 18 2016 10:10:45 GMT+0200 (FLE Standard Time)

module.exports = function (config) {
    var gulpConfig = require('./gulp.config')();

    config.set({

        // base path that will be used to resolve all patterns (eg. files, exclude)
        basePath: './',


        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ['jasmine'],


        // list of files / patterns to load in the browser
        files: gulpConfig.karma.files,

        // list of files to exclude
        exclude: gulpConfig.karma.exclude,

        plugins: [
            'karma-jasmine',
            'karma-chrome-launcher',
            'karma-firefox-launcher',
            'karma-jasmine-html-reporter',
            'karma-beep-reporter',
            'karma-coverage'
        ],

        // preprocess matching files before serving them to the browser
        // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
        preprocessors: gulpConfig.karma.preprocessors,

        // optionally, configure the reporter
        coverageReporter: {
          type : 'html',
          dir : 'coverage/'
        },
        // test results reporter to use
        // possible values: 'dots', 'progress'
        // available reporters: https://npmjs.org/browse/keyword/karma-reporter
        reporters: ['progress', 'kjhtml', 'coverage', 'beep'],
        // web server port
        port: 9876,


        // enable / disable colors in the output (reporters and logs)
        colors: true,


        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,


        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,


        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        browsers: ['Chrome'],

        captureTimeout: 60000,
        browserDisconnectTimeout: 60000,
        browserDisconnectTolerance: 10,
        browserNoActivityTimeout: 60000,
        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: false,

        // Concurrency level
        // how many browser should be started simultaneous
        concurrency: Infinity
    })
}