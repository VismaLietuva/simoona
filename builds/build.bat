@ECHO OFF

set simoonaOssPath=
set /p simoonaOssPath=Simoona open-source root path: || set simoonaOssPath=..\..\open-source

powershell -ExecutionPolicy ByPass -File build.ps1 -script build.cake -target="Default" -simoonaOssPath=%simoonaOssPath%