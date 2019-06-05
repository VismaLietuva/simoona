# Installation from Binaries

## Required Software

1. Internet Information Services (IIS) Server (<https://www.iis.net/>) - can be turned on as Windows feature
2. IIS URL Rewrite extension for IIS (<https://www.iis.net/downloads/microsoft/url-rewrite>)
3. SQL Server Express (<https://www.microsoft.com/en-us/sql-server/sql-server-editions-express>) - Basic version should be enough

## Installation Process

1. To download precompiled binaries, please navigate to [Simoona releases](https://github.com/VismaLietuva/simoona/releases) and download latest stable version package.
1. Unzip download packages where you would like to run Simoona from.
1. Execute `install.bat` *as Administrator* from unzipped package and provide MSSQL connection string when asked.
1. After successful installation Simoona should be accessible on `http://app.simoona.local`.

- Default organization name: `testorg`
- Default username: `tester@example.com`
- Default password: `testerPass123`
