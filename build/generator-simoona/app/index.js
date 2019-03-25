var Generator = require('yeoman-generator');

module.exports = class extends Generator {
    constructor(args, opts) {
      super(args, opts);
    }

    initializing() {
        var chalk = require('chalk');
        this.log('');
        this.log(chalk.bold.red('            @@@@@@@@             ') + chalk.bold.white('    @@@@@     @@@@   @@@@    @@@@@      :@@@@@   @@@@@@        @@@@'));
        this.log(chalk.bold.red('   @@@        @@@@@@@@@          ') + chalk.bold.white('     @@@@@   @@@@@   @@@@   @@@@@       r@@@@@: .@@@@@2       :@@@@@'));
        this.log(chalk.bold.red('  @@@@@@@      @@@@@@@@@@@       ') + chalk.bold.white('      @@@@   @@@@    @@@@   @@@@        i@@@@@h ;@@@@@M       @@@@@@'));
        this.log(chalk.bold.red('  @@@@@@@@@     @@@@@@@@@@@      ') + chalk.bold.white('      @@@@   @@@@    @@@@   @@@@@       h@@B@@@ X@@h@@@      .@@@@@@@'));
        this.log(chalk.bold.red('  @@@@@@@@@@@@    @@@@@@@@@@@    ') + chalk.bold.white('       @@@@ @@@@     @@@@    @@@@@      @@@rS@@ M@#,@@@      #@@Xi@@@'));
        this.log(chalk.bold.red('   @@@@@@@@@@@@@   @@@@@@@@@@@   ') + chalk.bold.white('       @@@@ @@@@     @@@@     @@@@@     @@@@,@@r@@G @@@.    .@@@. @@@@'));
        this.log(chalk.bold.red('    @@@@@@@@@@@@@@  @@@@@@@@@@@  ') + chalk.bold.white('        @@@@@@@      @@@@      @@@@@    @@@@ @@@@@@ @@@r    M@@@  @@@@'));
        this.log(chalk.bold.red('      @@@@@@@@@@@@@@ @@@@@@@@@@  ') + chalk.bold.white('        @@@@@@@      @@@@       @@@@   .@@@@ @@@@@  @@@h    @@@X  ;@@@@'));
        this.log(chalk.bold.red('        @@@@@@@@@@@@@@@@@@@@@@@  ') + chalk.bold.white('         @@@@@       @@@@       @@@@   ;@@@@ ;@@@@  @@@@   H@@@    @@@@'));
        this.log(chalk.bold.red('           @@@@@@@@@@@@@@@@@@    ') + chalk.bold.white('         @@@@@       @@@@       @@@@   9@@@@  @@@h  @@@@  .@@@#    S@@@5'));
        this.log(chalk.bold.red('               @@@@@@@@@@@@      ') + chalk.bold.white('          @@@        @@@@     @@@@@    X@@@,  h@@,  2@@@. 5@#@,     @@@@'));
        this.log('');
        this.log(chalk.bold.red('                               We love curious people. Get in touch! www.visma.com/careers'));
        this.log('');
    }

    prompting() {
        return this.prompt([{
          type: 'list',
          name: 'activity',
          message: 'Choose an option:',
          choices: [{
              name: 'do everything',
              value: 'Start'
            },{
              name: 'create database and resolve dependencies',
              value: 'OnlyDbAndDependencies'
            }
          ]
        }, {
          type: 'list',
          name: 'dropdb',
          message: 'Do you want to recreate database if it exists',
          choices: [{
              name: 'no',
              value: 'false'
            },{
              name: 'yes',
              value: 'true'
            }
          ]
        }, {
          type    : 'input',
          name    : 'organization',
          message : 'Enter organization name:',
          validate: function (organization){
            return organization !== '';
          }
        }, {
          type    : 'input',
          name    : 'email',
          message : 'Enter e-mail address for administrator account:',
          validate: function (email){
            var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            return email !== '' && re.test(String(email).toLowerCase());
          }
        }, {
          type    : 'input',
          name    : 'connectionString',
          message : 'Enter database server connection string (without database name):',
          validate: function (connectionString){
            var conn = connectionString.toLowerCase();
            return conn !== '';
          }
        }, {
          type    : 'input',
          name    : 'dbName',
          message : 'Enter database name for Simoona:',
          validate: function (dbName){
            return dbName !== '';
          }
        }]).then((answers) => {
          this.props = answers;
        });
      }

      install() {
        this.spawnCommandSync('powershell', ['./build.ps1', '-organization="' + this.props.organization + '"', '-email="' + this.props.email + '"', 
        '-connectionString="' + this.props.connectionString + '"', '-dbName="'+ this.props.dbName +'"', '-activity="' + this.props.activity + '"',
        '-dropdb="' + this.props.dropdb + '"']);
      }
  };