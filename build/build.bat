@ECHO OFF
cd /D "%~dp0"
call npm link ./generator-simoona
call yo simoona
PAUSE