# Simoona Web Application

Simoona's front-end is built using [AngularJS](https://angularjs.org/) framework. Project's dependencies are managed with [NPM](https://www.npmjs.com/) and [Bower](https://bower.io/).

## Gulp Command List

Gulp is used for automating and executing various commands. To use the command simply open command line inside `src/webapp/` folder and type `gulp <command name>`:

1. `gulp serve-dev`   - serves development environment and opens browser
2. `gulp serve-build` - serves build environment
3. `gulp build-dev`   - builds everything
4. `gulp build-prod`  - builds and optimizes everything

For all the commands open `src/webapp/gulpfile.js` file.

## Configuration

In order to change WebApp's configuration open file located in `src\webapp\gulp.config.js`. By default front-end has two different build configs one for `dev` environment and the other one for `prod` environment as shown below:

```javascript
 defaultBuildConfig: {
   endpoint: 'http://localhost:50321/',
   impersonate: false,
   showMissingTranslations: true,
   environment: 'dev'
 },
 productionBuildConfig: {
   endpoint:  'http://localhost:50321/',
   impersonate: false,
   showMissingTranslations: false,
   environment: 'prod'
 }
```

One of the most important properties is `endpoint` if your API project is running on a different address don't forget to change the config property accordingly.

## SSL Certificate

If you have SSL certificate file in `.pem` format you can put the certificate inside `src\webapp\src\server\` folder the name of that file should be `cert.pem`, you will also have to put RSA private key's file in the same folder, but the name of that file has to be `newkey.pem`.

You can change the naming and/or path of these files inside of `src\webapp\src\server\app.js` file, just open it and find the code snippet shown bellow:

```javascript
var sslKeyPath = './src/server/newkey.pem';
var sslCertPath = './src/server/cert.pem';
```

## Web Config

If you want to redirect all requests to HTTPS open `src\webapp\src\client\web.config` file and locate `Redirect production to https` rule. Replace `yourwebsite.url` with url you are using for your WebApp. The rule is shown bellow:

```xml
<rule name="Redirect production to https" stopProcessing="true">
  <match url="(.*)" />
  <conditions>
    <add input="{HTTP_HOST}" pattern="yourwebsite.url" ignoreCase="true" />
    <add input="{HTTPS}" pattern="^OFF$" ignoreCase="true" />
  </conditions>
  <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
</rule>
```