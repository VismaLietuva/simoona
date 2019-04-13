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
          type    : 'confirm',
          name    : 'hasAgreed',
          message : 'Are you sure you want to drop databases related to Simoona?'
        }, {
          type: 'list',
          name: 'bypassExecPolicy',
          message: 'Select powershell build script ExecutionPolicy',
          choices: [
            {
              name: 'Default',
              value: false
            }, {
              name: 'Bypass',
              value: true
            },
          ]
        }]).then((answers) => {
          this.props = answers;
        });
      }

      install() {
          if(this.props.hasAgreed) {
            this.spawnCommandSync(
              (this.props.bypassExecPolicy ?  'powershell  -ExecutionPolicy Bypass ' : 'powershell'), 
              ['./build.ps1', '-target="DropDatabases"']
            );
          }
      }
  };