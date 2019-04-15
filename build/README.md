# Build

The contents of this folder is used to make building and starting Simoona easier. For this purposes we are using [Yeoman](http://yeoman.io/) with a combination of [Cake](https://cakebuild.net/).

## Required Software

1. NPM and Node.js (<https://www.npmjs.com/get-npm>)
2. Yeoman (<http://yeoman.io>)
3. Bower (<https://bower.io>)
4. SQL Server Express (<https://www.microsoft.com/en-us/sql-server/sql-server-editions-express>)
5. IIS URL Rewrite (<https://www.iis.net/downloads/microsoft/url-rewrite>)
6. Git (<https://git-scm.com/downloads>)
7. Visual Studio, or Microsoft Build Tools 2015 (<https://www.microsoft.com/en-us/download/details.aspx?id=48159>) and Windows 8.x SDK (<https://developer.microsoft.com/en-us/windows/downloads/sdk-archive>)

## Installation Instructions

Build script has two main operations:

* **do everything**: creates the database for you using SQL scripts, inserts organization name and connection string to Web.config file, resolves API and WebApp dependencies, builds them and after some more steps launches Simoona.
* **create database and resolve dependencies**: just creates and inserts necessary data to database, modifies Web.config file then resolves project's dependencies. Use latter option if you want to build and launch Simoona using different commands or environments.

1. Setting up environment for the very first time: 
    * Open command line
    * Navigate to `build\generator-simoona`
    * Enter `npm link`
1. Run `build.bat` as Administrator and follow the "wizard".
1. Enter organization name (you will need to login to the app).
1. Enter default user e-mail address. You will use this e-mail address to sign-in. It will create a user with `testerPass123` as a password.
1. Enter database connection string (example: `Data Source=localhost\SQLEXPRESS;Integrated Security=True;Connect Timeout=60; MultipleActiveResultSets=True;`)
1. Enter dabase name (example: `SimoonaDB`)
1. Wait for build script to finish.

## Installation Troubleshooting

1. build\build.ps1 cannot be loaded. The file build\build.ps1 is not digitally signed. You cannot run this script on the current system. For more information about running scripts and setting execution policy, see about_Execution_Policies at https:/go.microsoft.com/fwlink/?LinkID=135170.

    Option a)

    * Select option in build script 'Select powershell build script ExecutionPolicy' -> Bypass

    Option b)

    * Right-click on build.ps1
    * Click on Properties
    * Check Unblock checkbox
    * Apply changes and click OK

1. An error occurred when executing task 'CreateApplicationPool'. Error: One or more errors occurred. Retrieving the COM class factory for component with CLSID {2B72133B-3F5B-4602-8952-803546CE3344} failed due to the following error: 80040154 Class not registered (Exception from HRESULT: 0x80040154 (REGDB_E_CLASSNOTREG)).

    * Click on Start Menu
    * Search for Turn Windows features on or off.
    * Turn on Internet Information Services

1. An error occurred when executing task 'CreateDatabase'. Error: One or more errors occurred. A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: SQL Network Interfaces, error: 26 - Error Locating Server/Instance Specified)

    * Install SQL Express (<https://www.microsoft.com/en-us/sql-server/sql-server-editions-express>)

1. C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Microsoft.Common.targets(2863,5): error MSB3086: Task could not find "AL.exe" using the SdkToolsPath "" or the registry key "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.0
A\WinSDK-NetFx40Tools-x86". Make sure the SdkToolsPath is set and the tool exists in the correct processor specific loc
ation under the SdkToolsPath and that the Microsoft Windows SDK is installed [..\api\Othe
r\Shrooms.Resources\Shrooms.Resources.csproj]

    * Make sure that Microsoft Build Tools 2015 and Windows SDK 8.x is installed 
creates the database for you using SQL scripts, inserts organization name and connection string to Web.config file, resolves API and WebApp dependencies, builds them and after some more steps launches Simoona.

1. HTTP Error 500.19

    * Install IIS URL Rewrite (<https://www.iis.net/downloads/microsoft/url-rewrite>)

1. HTTP Error 401.3 - Unauthorized You do not have permission to view this directory or page because of the access control list (ACL) configuration or encryption settings for this resource on the Web server.

    * Right-click Simoona source folder
    * Click on Security tab
    * Click on Edit
    * Click on Add
    * Enter Authenticated Users in Enter the object names to select text box  

1. HTTP500: SERVER ERROR - The server encountered an unexpected condition that prevented it from fulfilling the request. (XHR)OPTIONS - <http://localhost:50321/Account/ExternalLogins?returnUrl=>

    * Click on Start Menu
    * Search for Turn Windows features on or off
    * Expand Internet Information Services
    * Expand World Wide Web Services
    * Expand Application Development Features
    * Make sure these features are turned on:
        * .NET Extensibility 3.5
        * .NET Extensibility 4.7
        * ASP.NET 3.5
        * ASP.NET 4.7
        * ISAPI Extensions
        * ISAPI Filters

1. HTTP Error 500.19 - Internal Server Error This configuration section cannot be used at this path. This happens when the section is locked at a parent level. Locking is either by default (overrideModeDefault="Deny"), or set explicitly by a location tag with overrideMode="Deny" or the legacy allowOverride="false".

    * Click on Start Menu
    * Search for Turn Windows features on or off
    * Expand Internet Information Services
    * Expand World Wide Web Services
    * Expand Application Development Features
    * Make sure these features are turned on:
        * .NET Extensibility 3.5
        * .NET Extensibility 4.7
        * ASP.NET 3.5
        * ASP.NET 4.7
        * ISAPI Extensions
        * ISAPI Filters
        
1. build.bat hangs on "Preparing to run build script..."

    * Inspect your NuGet package sources. One way of doing that is by running "nuget sources" command
    * Disable NuGet package sources that require authentication. Ideally, you should enable only nuget.org as a source
    * Run build.bat as administrator
    * Enable the disabled packages after installation is complete

    
