@echo off
setlocal EnableExtensions

if "%~3"=="" (
    echo Usage: server-host.cmd ^<app-dir^> ^<project-path^> ^<log-file^>
    exit /b 1
)

title OWASP API Security Demo Server
cd /d "%~1"
dotnet run --project "%~2" > "%~3" 2>&1
exit /b %ERRORLEVEL%
