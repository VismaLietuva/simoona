@ECHO OFF

set rootPath=
set simoonaOssPath=

set /p rootPath=Provide root path, where Premium project source is located (default is "..\src"): || set rootPath=..\src
set /p simoonaOssPath=Provide Premium project relative path to Simoona open-source root (default is "..\..\open-source"): || set simoonaOssPath=..\..\open-source

powershell -ExecutionPolicy ByPass -File build.ps1 -script build.cake -target="Default" -rootPath=%rootPath% -simoonaOssPath=%simoonaOssPath%