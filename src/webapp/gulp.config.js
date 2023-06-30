module.exports = function () {
    var client = './src/client/';
    var server = './src/server/';
    var clientApp = client + 'app/';
    var clientTests = client + 'tests/';
    var report = './report/';
    var root = './';
    var temp = './.tmp/';
    var wiredep = require('wiredep');
    var bowerFiles = wiredep({
        devDependencies: true
    })['js'];
    var bower = {
        json: require('./bower.json'),
        directory: './bower_components/',
        ignorePath: '../..'
    };
    var nodeModules = 'node_modules';

    var config = {
        clientApp: clientApp,
        /**
         * File paths
         */
        // all javascript that we want to vet
        alljs: [
            './src/**/*.js',
            './*.js'
        ],
        build: './build/',
        defaultBuildConfig: {
            endpoint: 'http://localhost:50321/',
            chatBotEndpoint: 'chatBotEndpointValue',
            chatBotAgentId: 'chatBotAgentIdValue',
            impersonate: false,
            showMissingTranslations: true,
            environment: 'dev'
        },
        productionBuildConfig: {
            endpoint:  'http://localhost:50321/',
            chatBotEndpoint: 'chatBotEndpointValue',
            chatBotAgentId: 'chatBotAgentIdValue',
            impersonate: false,
            showMissingTranslations: false,
            environment: 'prod'
        },
        prod_extras: [
          client + '*.*',
          '!' + client + '*.html',
          '!' + client + 'robots.txt'
        ],

        client: client,
        css: temp + 'styles.css',
        cssBootstrap: temp + 'bootstrap.css',
        fonts: client + '/fonts/**/*.*',
        html: client + '**/*.html',
        htmltemplates: clientApp + '**/*.html',
        images: client + 'images/**/*.*',
        resources: client + 'resources/**/*.*',
        angularLocales: client + 'resources/*.js',
        locales: ['lt_LT', 'en_US'],
        extras: [
          client + '*.*',
          '!' + client + '*.html'
        ],
        index: client + 'index.html',
        // app js, with no specs
        js: [
            clientApp + '**/config.js',
            clientApp + '**/routing.js',
            clientApp + '**/build-config.js',
            clientApp + '**/*.module.js',
            clientApp + '**/*.js',
            '!' + clientApp + '**/*-spec.js',
            '!' + clientApp + '**/*-mocks.js'
        ],
        jsOrder: [
            '**/config.js',
            '**/routing.js',
            '**/*.module.js',
            '**/*.js',
            '**/**/*.js'
        ],
        less: client + 'styles/style/styles.less',
        bootstrapLess: client + 'styles/bootstrap/bootstrap.less',
        allLess: client + 'styles/**/*.less',
        report: report,
        root: root,
        server: server,
        source: 'src/',

        //stubsjs: [
        //bower.directory + 'angular-mocks/angular-mocks.js',
        //client + 'stubs/**/*.js'
        //],

        temp: temp,

        /**
         * optimized files
         */
        optimized: {
            app: 'app.js',
            toplib: 'toplibs.js',
            botlib: 'botlibs.js'
        },

        /**
         * browser sync
         */
        browserReloadDelay: 1000,

        /**
         * template cache
         */
        templateCache: {
            file: 'templates.js',
            options: {
                module: 'simoonaApp',
                root: 'app/',
                standAlone: false
            }
        },

        /**
         * Bower and NPM files
         */
        bower: bower,
        packages: [
            './package.json',
            './bower.json'
        ],

        /**
         * The sequence of the injections into specs.html:
         *  1 testlibraries
         *      mocha setup
         *  2 bower
         *  3 js
         *  4 spechelpers
         *  5 specs
         *  6 templates
         */
        testlibraries: [
            nodeModules + '/jasmine/bin/jasmine.js'
        ],
        specMocks: [clientTests + '**/*-mocks.js'],
        specs: [clientTests + '**/*-spec.js'],

        /**
         * Node settings
         */
        nodeServer: './src/server/app.js',
        defaultPort: '7203'
    };

    config.FeatureFlag = {
            'key': '',
            'active': '',
            'name': '',
            'description': 'hidden feature'
        };


    /**
     * wiredep and bower settings
     */
    config.getWiredepDefaultOptions = function () {
        var options = {
            bowerJson: config.bower.json,
            directory: config.bower.directory,
            ignorePath: config.bower.ignorePath
        };
        return options;
    };

    /**
     * karma settings
     */
    config.karma = getKarmaOptions();

    return config;

    ////////////////

    function getKarmaOptions() {
        var options = {
            files: [].concat(
                bowerFiles,
                client + 'Libs/Chart.min.js',
                client + 'Libs/angular/*.js',
                client + 'Libs/jquery/*.js',
                client + 'Libs/**/*.js',
                clientApp + '**/config.js',
                clientApp + '**/routing.js',
                clientApp + '**/build-config.js',
                clientApp + '**/*.module.js',
                clientApp + '**/*.js',
                temp + config.templateCache.file,
                config.specMocks,
                config.specs
            ),
            exclude: [],
            coverage: {
                dir: report + 'coverage',
                reporters: [
                    // reporters not supporting the `file` property
                    {
                        type: 'html',
                        subdir: 'report-html'
                    },
                    {
                        type: 'lcov',
                        subdir: 'report-lcov'
                    },
                    // reporters supporting the `file` property, use `subdir` to directly
                    // output them in the `dir` directory.
                    // omit `file` to output to the console.
                    // {type: 'cobertura', subdir: '.', file: 'cobertura.txt'},
                    // {type: 'lcovonly', subdir: '.', file: 'report-lcovonly.txt'},
                    // {type: 'teamcity', subdir: '.', file: 'teamcity.txt'},
                    //{type: 'text'}, //, subdir: '.', file: 'text.txt'},
                    {
                        type: 'text-summary'
                    } //, subdir: '.', file: 'text-summary.txt'}
                ]
            },
            preprocessors: {}
        };
        options.preprocessors[clientApp + '**/*.js'] = ['coverage'];
        return options;
    }
};
