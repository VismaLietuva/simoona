/*jshint node:true*/
'use strict';
var https = require('https');
var express = require('express');
var app = express();
var bodyParser = require('body-parser');
var compress = require('compression');
var cors = require('cors');
var errorHandler = require('./utils/errorHandler')();
//var forceSSL = require('express-force-ssl');
var four0four = require('./utils/404')();
var favicon = require('serve-favicon');
var logger = require('morgan');
var fs = require('fs');

var secureServer;
try {
  var ssl_options = {
    key: fs.readFileSync('./src/server/newkey.pem'),
    cert: fs.readFileSync('./src/server/cert.pem')
  };
  secureServer = https.createServer(ssl_options, app);
  console.log('Creating server with SSL');
} catch (error) {
  secureServer = https.createServer(app);
  console.log('Creating server without SSL');
}

var port = process.env.PORT || 7203;

var environment = process.env.NODE_ENV;
var etag = {etag: true};

app.use(favicon(__dirname + '/favicon.ico'));
app.use(bodyParser.urlencoded({extended: true}));
app.use(bodyParser.json());
//app.use(forceSSL);
app.use(compress());
app.use(logger('dev'));
app.use(cors());
app.use(errorHandler.init);

console.log('About to crank up node');
console.log('PORT=' + port);
console.log('NODE_ENV=' + environment);

app.get('/ping', function(req, res, next) {
    console.log(req.body);
    res.send('pong');
});

switch (environment) {
    case 'build':
        console.log('** BUILD **');
        app.use(express.static('./build/', etag));
        // Any invalid calls for templateUrls are under app/* and should return 404
        app.use('/app/*', function(req, res, next) {
            four0four.send404(req, res);
        });
        // Any deep link calls should return index.html
        app.use('/*', express.static('./build/index.html'));
        break;
    default:
        console.log('** DEV **');
        app.use(express.static('./src/client/', etag));
        app.use(express.static('./',
            {etag: true/*, maxAge: 8640000*/}
        ));
        app.use(express.static('./.tmp', etag));
        // All the assets are served at this point.
        // Any invalid calls for templateUrls are under app/* and should return 404
        app.use('/app/*', function(req, res, next) {
            four0four.send404(req, res);
        });
        // Any deep link calls should return index.html
        app.use('/*', express.static('./src/client/index.html', etag));


        break;
}

secureServer.listen(44330);

app.listen(port, function() {
    console.log('Express server listening on port ' + port);
    console.log('env = ' + app.get('env') +
                '\n__dirname = ' + __dirname +
                '\nprocess.cwd = ' + process.cwd());
});