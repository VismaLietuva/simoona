@ECHO OFF

set ossPathForked=
set /p ossPathForked=Forked Simoona open-source root path (where Git repository is cloned): || set ossPathForked=..\..\open-source

powershell -ExecutionPolicy ByPass -File build.ps1 -script build.cake -target="Default" -ossPathForked=%ossPathForked%