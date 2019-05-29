@ECHO OFF
@setlocal enableextensions
@cd /d "%~dp0"

set connectionString=
set /p connectionString=Please provide ConnectionString (default: Data Source=localhost\SQLEXPRESS;Integrated Security=True;Connect Timeout=60; MultipleActiveResultSets=True;): || set connectionString=Data Source=localhost\SQLEXPRESS;Integrated Security=True;Connect Timeout=60; MultipleActiveResultSets=True;

powershell -ExecutionPolicy ByPass -File build.ps1 -script build.cake -target="Default" -connectionString="%connectionString%"
PAUSE